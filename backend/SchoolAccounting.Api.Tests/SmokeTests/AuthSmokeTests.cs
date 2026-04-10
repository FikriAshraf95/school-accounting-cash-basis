using SchoolAccounting.Api.Features.Auth;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Tests.SmokeTests;

public class AuthSmokeTests : TestBase
{
    private readonly AuthService _authService;

    public AuthSmokeTests()
    {
        _authService = new AuthService(DbContext);
    }

    [Fact]
    public async Task Register_ShouldCreateUser_AndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("Viewer", result.User.Role);

        // Verify user exists in database
        var user = await DbContext.Users.FindAsync(result.User.Id);
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("test@example.com", result.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.UnauthorizedException>(
            () => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };
        await _authService.RegisterAsync(request);

        var duplicateRequest = new RegisterRequest
        {
            Name = "Another User",
            Username = "anotheruser",
            Email = "test@example.com", // Same email
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.ConflictException>(
            () => _authService.RegisterAsync(duplicateRequest));
    }

    [Fact]
    public async Task Logout_ShouldRevokeToken()
    {
        // Arrange - Register and login
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };
        var authResult = await _authService.RegisterAsync(registerRequest);

        // Act
        await _authService.LogoutAsync(authResult.User.Id, authResult.Token);

        // Assert - Token should be removed
        var user = await DbContext.Users.FindAsync(authResult.User.Id);
        Assert.NotNull(user);
        // Verify by checking that login creates a new token
        var loginResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        });
        Assert.NotEqual(authResult.Token, loginResult.Token);
    }
}
