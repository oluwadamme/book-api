using Moq;
using FirstApi.Services;
using FirstApi.Models;
using FirstApi.DTOs;
using FirstApi.Repositories.Interfaces;
using FirstApi.Options;
using Microsoft.Extensions.Options;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;


namespace FirstApi.Tests.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepository;
    private readonly Mock<IOptions<EmailVerificationOptions>> _mockEmailOptions;
    private readonly Mock<IOptions<JwtOptions>> _mockJwtOptions;
    private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
    private readonly AuthService _authService;


    public AuthServiceTests()
    {
        _mockAuthRepository = new Mock<IAuthRepository>();

        _mockEmailOptions = new Mock<IOptions<EmailVerificationOptions>>();
        _mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
        _authService = new AuthService(_mockAuthRepository.Object, _mockEmailOptions.Object, _mockJwtOptions.Object, _mockBackgroundJobClient.Object);
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

        // Mock: IOptions<EmailVerificationOptions>
        _mockEmailOptions.Setup(o => o.Value).Returns(new EmailVerificationOptions
        {
            ExpirationInMinutes = 30
        });

        // Mock: AddUserAsync accepts any User (returns Task)
        _mockAuthRepository.Setup(s => s.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.False(result.IsEmailVerified);
        _mockAuthRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        _mockBackgroundJobClient.Verify(x => x.Create(
            It.Is<Job>(j => j.Method.Name == "SendEmailAsync"),
            It.IsAny<IState>()), Times.Once);
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

        // Mock: IOptions<JwtOptions>
        _mockJwtOptions.Setup(o => o.Value).Returns(new JwtOptions
        {
            Key = "ThisIsASecretKeyForTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 30
        });

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