namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class StudentClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public StudentGrade Grade { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = [];
}
