namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class Ledger
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // asset / liability / equity / revenue / expense
    public string? Category { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Category> Categories { get; set; } = [];
}
