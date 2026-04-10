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
