using Moq;
using Xunit;
using FirstApi.Services;
using FirstApi.Models;
using FirstApi.Data;
using FirstApi.DTOs;
using FirstApi.Repositories.Interfaces;
using FirstApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FirstApi.Tests.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockAuthRepository = new Mock<IAuthRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _authService = new AuthService(_mockAuthRepository.Object,_mockConfiguration.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidUser_ReturnsUserDto()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Pass1@word",
            FirstName = "John",
            LastName = "Doe"
        };

        // Mock: user does not already exist
        _mockAuthRepository.Setup(s => s.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);

        // Mock: IConfiguration for EmailVerification expiry
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s["ExpirationInMinutes"]).Returns("30");
        _mockConfiguration.Setup(c => c.GetSection("EmailVerification")).Returns(mockSection.Object);

        // Mock: AddUserAsync accepts any User (returns Task)
        _mockAuthRepository.Setup(s => s.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Mock: SendEmailAsync
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.False(result.IsEmailVerified);
        _mockAuthRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExistingEmail_ThrowsException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe"
        };

        // Mock: user already exists
        _mockAuthRepository.Setup(s => s.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _authService.RegisterUserAsync(request));
        Assert.Equal("User already exists", exception.Message);
    }

    [Fact]
    public async Task LoginUserAsync_ValidCredentials_ReturnsUserDtoWithToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        // Mock: user exists
        _mockAuthRepository.Setup(s => s.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);

        // Mock: IConfiguration for JWT settings
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(s => s["Key"]).Returns("ThisIsASecretKeyForTestingPurposes123!");
        mockJwtSection.Setup(s => s["ExpirationInMinutes"]).Returns("60");
        mockJwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        mockJwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(mockJwtSection.Object);

        // Act
        var result = await _authService.LoginUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.NotNull(result.Token);
    }
}