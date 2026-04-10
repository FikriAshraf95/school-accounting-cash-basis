namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class Transaction
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty; // income / expense
    public string TransactableType { get; set; } = string.Empty; // "Student" or "Payer"
    public int? StudentId { get; set; }
    public int? PayerId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public int CashLedgerId { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversalTransactionId { get; set; }
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Student? Student { get; set; }
    public Payer? Payer { get; set; }
    public Ledger CashLedger { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
    public Transaction? ReversalTransaction { get; set; }
    public List<TransactionItem> Items { get; set; } = [];
    public List<JournalEntry> JournalEntries { get; set; } = [];
}
