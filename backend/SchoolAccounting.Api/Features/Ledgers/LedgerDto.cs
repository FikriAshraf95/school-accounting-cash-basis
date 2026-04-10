namespace SchoolAccounting.Api.Features.Ledgers;

public class CreateLedgerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // asset / liability / equity / revenue / expense
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLedgerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LedgerResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TrialBalanceRequest
{
    public int? Year { get; set; }
}

public class TrialBalanceResponse
{
    public List<TrialBalanceItemResponse> Items { get; set; } = [];
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
}

public class TrialBalanceItemResponse
{
    public int LedgerId { get; set; }
    public string LedgerCode { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

public class LedgerSummaryRequest
{
    public int Year { get; set; }
}

public class LedgerSummaryResponse
{
    public int Year { get; set; }
    public List<LedgerSummaryItemResponse> Ledgers { get; set; } = [];
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
}

public class LedgerSummaryItemResponse
{
    public int LedgerId { get; set; }
    public string LedgerCode { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal NetChange { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class YearEndCloseRequest
{
    public int Year { get; set; }
}

public class YearEndCloseResponse
{
    public int Year { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal NetExpense { get; set; }
    public decimal NetProfit { get; set; }
    public List<ClosingEntryResponse> ClosingEntries { get; set; } = [];
    public DateTime ClosedAt { get; set; }
}

public class ClosingEntryResponse
{
    public string LedgerCode { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class YearBeginningOpenRequest
{
    public int Year { get; set; }
}

public class YearBeginningOpenResponse
{
    public int Year { get; set; }
    public decimal TotalOpeningDebits { get; set; }
    public decimal TotalOpeningCredits { get; set; }
    public bool IsBalanced { get; set; }
    public List<OpeningEntryResponse> OpeningEntries { get; set; } = [];
    public DateTime OpenedAt { get; set; }
}

public class OpeningEntryResponse
{
    public string LedgerCode { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
