namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class PersonalAccessToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;  // SHA-256 hash
    public string Name { get; set; } = string.Empty;   // device/client label
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
