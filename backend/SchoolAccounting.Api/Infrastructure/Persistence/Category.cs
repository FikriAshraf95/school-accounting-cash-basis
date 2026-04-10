namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // income / expense
    public int LedgerId { get; set; }
    public string? Description { get; set; }
    public bool RequiresStudent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Ledger Ledger { get; set; } = null!;
}
