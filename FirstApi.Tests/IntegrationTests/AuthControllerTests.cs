using System.Net;
using System.Net.Http.Json;
using FirstApi.DTOs;
namespace FirstApi.Tests.IntegrationTests;
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
  
    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    [Fact]
    public async Task Register_ValidUser_Returns200()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",  // unique email each run
            Password = "Password123!"
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse<UserDto>>();
        Assert.True(body?.Success);
        Assert.Equal(request.Email, body?.Data?.Email);
    }
    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };
        // Register first time
        await _client.PostAsJsonAsync("/api/Auth/register", request);
        // Act — register again with same email
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task GetBooks_WithoutToken_Returns401()
    {
        // Act — no Authorization header
        var response = await _client.GetAsync("/api/Books");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}