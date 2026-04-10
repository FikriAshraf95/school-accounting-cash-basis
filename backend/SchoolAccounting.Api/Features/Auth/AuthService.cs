using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;
using System.Security.Cryptography;

namespace SchoolAccounting.Api.Features.Auth;

public class AuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new ConflictException("Email already registered");
        }

        // Check if username already exists
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new ConflictException("Username already taken");
        }

        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = AppRole.Viewer, // Default role
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = await CreateTokenAsync(user);

        return new AuthResponse
        {
            Token = token,
            User = user.ToResponse()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        var token = await CreateTokenAsync(user);

        return new AuthResponse
        {
            Token = token,
            User = user.ToResponse()
        };
    }

    public async Task LogoutAsync(int userId, string token)
    {
        var tokenHash = HashToken(token);

        var accessToken = await _dbContext.PersonalAccessTokens
            .FirstOrDefaultAsync(t => t.Token == tokenHash && t.UserId == userId);

        if (accessToken != null)
        {
            _dbContext.PersonalAccessTokens.Remove(accessToken);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<UserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found");

        return user.ToResponse();
    }

    private async Task<string> CreateTokenAsync(User user)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();
        var tokenHash = HashToken(token);

        var accessToken = new PersonalAccessToken
        {
            UserId = user.Id,
            Token = tokenHash,
            Name = "API Token",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // 30 day expiry
        };

        _dbContext.PersonalAccessTokens.Add(accessToken);
        await _dbContext.SaveChangesAsync();

        return token;
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public static class AuthMappings
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
