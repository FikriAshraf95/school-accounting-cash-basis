namespace SchoolAccounting.Api.Features.Transactions;

public class CreateTransactionRequest
{
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty; // income / expense
    public string TransactableType { get; set; } = string.Empty; // "Student" or "Payer"
    public int? StudentId { get; set; }
    public int? PayerId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public int CashLedgerId { get; set; }
    public List<CreateTransactionItemRequest> Items { get; set; } = [];
}

public class CreateTransactionItemRequest
{
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class UpdateTransactionRequest
{
    public DateTime TransactionDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public List<CreateTransactionItemRequest> Items { get; set; } = [];
}

public class TransactionResponse
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TransactableType { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? PayerId { get; set; }
    public string? PayerName { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public int CashLedgerId { get; set; }
    public string? CashLedgerName { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversalTransactionId { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TransactionItemResponse> Items { get; set; } = [];
}

public class TransactionItemResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class TransactionListItemResponse
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? TransactableName { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public bool IsReversed { get; set; }
    public DateTime CreatedAt { get; set; }
}
