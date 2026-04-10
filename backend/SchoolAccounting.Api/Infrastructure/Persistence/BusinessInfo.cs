namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class BusinessInfo
{
    public int Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly FinancialYearStart { get; set; }
    public DateOnly FinancialYearEnd { get; set; }
    public string Currency { get; set; } = "PHP";
    public string Timezone { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? TaxRegistration { get; set; }
    public decimal TaxRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
