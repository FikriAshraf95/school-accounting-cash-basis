namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class Payer
{
    public int Id { get; set; }
    public string PayerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // donor / sponsor / vendor / supplier / general / government
    public string Category { get; set; } = string.Empty; // individual / corporate / government / ngo
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
