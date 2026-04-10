namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class ClosedYear
{
    public int Id { get; set; }
    public int Year { get; set; }
    public DateTime ClosedAt { get; set; }
    public int ClosedBy { get; set; }
    
    public User ClosedByUser { get; set; } = null!;
}
