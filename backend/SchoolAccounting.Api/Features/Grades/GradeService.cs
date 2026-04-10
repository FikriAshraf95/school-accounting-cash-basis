using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Grades;

public class GradeService
{
    private readonly AppDbContext _dbContext;

    public GradeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<GradeResponse>> GetGradesAsync(int page, int perPage, string? sortBy, bool sortDesc)
    {
        var query = _dbContext.StudentGrades
            .Where(g => g.DeletedAt == null)
            .AsQueryable();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name),
            "code" => sortDesc ? query.OrderByDescending(g => g.Code) : query.OrderBy(g => g.Code),
            "isactive" => sortDesc ? query.OrderByDescending(g => g.IsActive) : query.OrderBy(g => g.IsActive),
            _ => sortDesc ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt)
        };

        var total = await query.CountAsync();
        var lastPage = (int)Math.Ceiling((double)total / perPage);

        var grades = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return new PagedResult<GradeResponse>
        {
            Data = grades.Select(g => g.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage
            }
        };
    }

    public async Task<GradeResponse> GetGradeAsync(int id)
    {
        var grade = await _dbContext.StudentGrades
            .Where(g => g.Id == id && g.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Grade with ID {id} not found");

        return grade.ToResponse();
    }

    public async Task<GradeResponse> CreateGradeAsync(CreateGradeRequest request)
    {
        // Check if code already exists
        if (await _dbContext.StudentGrades.AnyAsync(g => g.Code == request.Code && g.DeletedAt == null))
        {
            throw new ConflictException($"Grade with code '{request.Code}' already exists");
        }

        var grade = new StudentGrade
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.StudentGrades.Add(grade);
        await _dbContext.SaveChangesAsync();

        return grade.ToResponse();
    }

    public async Task<GradeResponse> UpdateGradeAsync(int id, UpdateGradeRequest request)
    {
        var grade = await _dbContext.StudentGrades
            .Where(g => g.Id == id && g.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Grade with ID {id} not found");

        // Check if code is being changed and if it's already taken
        if (grade.Code != request.Code && await _dbContext.StudentGrades.AnyAsync(g => g.Code == request.Code && g.DeletedAt == null && g.Id != id))
        {
            throw new ConflictException($"Grade with code '{request.Code}' already exists");
        }

        grade.Name = request.Name;
        grade.Code = request.Code;
        grade.Description = request.Description;
        grade.IsActive = request.IsActive;
        grade.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return grade.ToResponse();
    }

    public async Task DeleteGradeAsync(int id)
    {
        var grade = await _dbContext.StudentGrades
            .Where(g => g.Id == id && g.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Grade with ID {id} not found");

        // Check if there are any classes using this grade
        var hasClasses = await _dbContext.StudentClasses.AnyAsync(c => c.GradeId == id && c.DeletedAt == null);
        if (hasClasses)
        {
            throw new ConflictException("Cannot delete grade that has associated classes");
        }

        grade.DeletedAt = DateTime.UtcNow;
        grade.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}

public static class GradeMappings
{
    public static GradeResponse ToResponse(this StudentGrade grade)
    {
        return new GradeResponse
        {
            Id = grade.Id,
            Name = grade.Name,
            Code = grade.Code,
            Description = grade.Description,
            IsActive = grade.IsActive,
            CreatedAt = grade.CreatedAt,
            UpdatedAt = grade.UpdatedAt
        };
    }
}
