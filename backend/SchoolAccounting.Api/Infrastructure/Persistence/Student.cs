namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class Student
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public int? GradeId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public StudentClass? Class { get; set; }
    public StudentGrade? Grade { get; set; }
}
