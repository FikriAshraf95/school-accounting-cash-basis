namespace SchoolAccounting.Api.Features.Students;

public class CreateStudentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public int? GradeId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStudentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public int? GradeId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AssignStudentToClassRequest
{
    public int ClassId { get; set; }
}

public class StudentResponse
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public int? GradeId { get; set; }
    public string? GradeName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StudentDetailResponse : StudentResponse
{
    public List<StudentTransactionSummaryResponse> TransactionHistory { get; set; } = [];
}

public class StudentTransactionSummaryResponse
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

public class StudentReportRequest
{
    public int? GradeId { get; set; }
    public int? ClassId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class StudentReportResponse
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? GradeName { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public int TransactionCount { get; set; }
}

public class StudentImportResult
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}
