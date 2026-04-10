namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class TransactionItem
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Transaction Transaction { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
