namespace SchoolAccounting.Api.Features.Payers;

public class CreatePayerRequest
{
    public string PayerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // donor / sponsor / vendor / supplier / general / government
    public string Category { get; set; } = string.Empty; // individual / corporate / government / ngo
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePayerRequest
{
    public string PayerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PayerResponse
{
    public int Id { get; set; }
    public string PayerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PayerDetailResponse : PayerResponse
{
    public List<PayerTransactionSummaryResponse> TransactionHistory { get; set; } = [];
}

public class PayerTransactionSummaryResponse
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool IsReversed { get; set; }
    public DateTime CreatedAt { get; set; }
}
