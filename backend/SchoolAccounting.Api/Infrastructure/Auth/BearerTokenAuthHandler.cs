using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolAccounting.Api.Infrastructure.Persistence;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SchoolAccounting.Api.Infrastructure.Auth;

public class BearerTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _dbContext;

    public BearerTokenAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AppDbContext dbContext) : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return AuthenticateResult.Fail("Authorization header missing");
        }

        var headerValue = authorizationHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid authorization scheme");
        }

        var token = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Token missing");
        }

        // Hash the token for lookup
        var tokenHash = HashToken(token);

        var accessToken = await _dbContext.PersonalAccessTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == tokenHash);

        if (accessToken == null)
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        if (accessToken.ExpiresAt.HasValue && accessToken.ExpiresAt.Value < DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("Token expired");
        }

        // Update last used timestamp
        accessToken.LastUsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accessToken.User.Id.ToString()),
            new Claim(ClaimTypes.Name, accessToken.User.Username),
            new Claim(ClaimTypes.Email, accessToken.User.Email),
            new Claim(ClaimTypes.Role, accessToken.User.Role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string HashToken(string token)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
