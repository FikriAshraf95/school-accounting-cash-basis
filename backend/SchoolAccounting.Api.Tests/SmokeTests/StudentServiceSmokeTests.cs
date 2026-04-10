using SchoolAccounting.Api.Features.Students;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Tests.SmokeTests;

public class StudentServiceSmokeTests : TestBase
{
    private readonly StudentService _studentService;

    public StudentServiceSmokeTests()
    {
        _studentService = new StudentService(DbContext);
    }

    [Fact]
    public async Task CreateStudent_ShouldSucceed()
    {
        var request = new CreateStudentRequest
        {
            StudentId = "STU001",
            Name = "Ahmad bin Abdullah",
            IsActive = true
        };

        var result = await _studentService.CreateStudentAsync(request);

        Assert.NotNull(result);
        Assert.Equal("STU001", result.StudentId);
        Assert.Equal("Ahmad bin Abdullah", result.Name);
        Assert.Equal(0, result.Balance);
    }

    [Fact]
    public async Task CreateStudent_DuplicateStudentId_ShouldThrowConflict()
    {
        var request = new CreateStudentRequest { StudentId = "STU001", Name = "First Student", IsActive = true };
        await _studentService.CreateStudentAsync(request);

        var duplicate = new CreateStudentRequest { StudentId = "STU001", Name = "Second Student", IsActive = true };

        await Assert.ThrowsAsync<Common.Exceptions.ConflictException>(
            () => _studentService.CreateStudentAsync(duplicate));
    }

    [Fact]
    public async Task GetStudents_ShouldReturnPagedResults()
    {
        DbContext.Students.AddRange(
            new Student { StudentId = "S001", Name = "Alice", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Student { StudentId = "S002", Name = "Bob", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Student { StudentId = "S003", Name = "Carol", Balance = 0, IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await DbContext.SaveChangesAsync();

        var all = await _studentService.GetStudentsAsync();
        var activeOnly = await _studentService.GetStudentsAsync(isActive: true);

        Assert.Equal(3, all.Meta.Total);
        Assert.Equal(2, activeOnly.Meta.Total);
    }

    [Fact]
    public async Task DeleteStudent_ShouldSoftDelete()
    {
        var created = await _studentService.CreateStudentAsync(
            new CreateStudentRequest { StudentId = "STU-DEL", Name = "To Delete", IsActive = true });

        await _studentService.DeleteStudentAsync(created.Id);

        // Soft-deleted student should not appear in list (global query filter applied)
        var result = await _studentService.GetStudentsAsync();
        Assert.DoesNotContain(result.Data, s => s.Id == created.Id);

    }

    [Fact]
    public async Task GetStudent_AfterDelete_ShouldThrowNotFound()
    {
        var created = await _studentService.CreateStudentAsync(
            new CreateStudentRequest { StudentId = "STU-DEL2", Name = "To Delete 2", IsActive = true });

        await _studentService.DeleteStudentAsync(created.Id);

        await Assert.ThrowsAsync<Common.Exceptions.NotFoundException>(
            () => _studentService.GetStudentAsync(created.Id));
    }
}
