namespace SchoolAccounting.Api.Features.Classes;

public class CreateClassRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateClassRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AssignStudentRequest
{
    public int StudentId { get; set; }
}

public class ClassResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClassDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsActive { get; set; }
    public List<StudentInClassResponse> Students { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StudentInClassResponse
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
