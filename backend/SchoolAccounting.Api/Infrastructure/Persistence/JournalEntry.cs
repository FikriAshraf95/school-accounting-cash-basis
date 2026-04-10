namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class JournalEntry
{
    public int Id { get; set; }
    public int? TransactionId { get; set; }
    public int LedgerId { get; set; }
    public string EntryType { get; set; } = string.Empty; // debit / credit
    public decimal Amount { get; set; }
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public string JournalType { get; set; } = string.Empty; // transaction / reversal / adjustment / closing / opening
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Transaction? Transaction { get; set; }
    public Ledger Ledger { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
