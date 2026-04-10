using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Classes;

public class ClassService
{
    private readonly AppDbContext _dbContext;

    public ClassService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ClassResponse>> GetClassesAsync(int page, int perPage, string? sortBy, bool sortDesc)
    {
        var query = _dbContext.StudentClasses
            .Where(c => c.DeletedAt == null)
            .Include(c => c.Grade)
            .AsQueryable();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "code" => sortDesc ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "grade" => sortDesc ? query.OrderByDescending(c => c.Grade.Name) : query.OrderBy(c => c.Grade.Name),
            "isactive" => sortDesc ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
            _ => sortDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var total = await query.CountAsync();
        var lastPage = (int)Math.Ceiling((double)total / perPage);

        var classes = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return new PagedResult<ClassResponse>
        {
            Data = classes.Select(c => c.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage
            }
        };
    }

    public async Task<ClassDetailResponse> GetClassAsync(int id)
    {
        var classEntity = await _dbContext.StudentClasses
            .Where(c => c.Id == id && c.DeletedAt == null)
            .Include(c => c.Grade)
            .Include(c => c.Students.Where(s => s.DeletedAt == null))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {id} not found");

        return classEntity.ToDetailResponse();
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request)
    {
        // Check if grade exists
        var grade = await _dbContext.StudentGrades
            .Where(g => g.Id == request.GradeId && g.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Grade with ID {request.GradeId} not found");

        // Check if code already exists
        if (await _dbContext.StudentClasses.AnyAsync(c => c.Code == request.Code && c.DeletedAt == null))
        {
            throw new ConflictException($"Class with code '{request.Code}' already exists");
        }

        var classEntity = new StudentClass
        {
            Name = request.Name,
            Code = request.Code,
            GradeId = request.GradeId,
            Section = request.Section,
            Description = request.Description,
            Capacity = request.Capacity,
            FeeAmount = request.FeeAmount,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.StudentClasses.Add(classEntity);
        await _dbContext.SaveChangesAsync();

        // Reload with grade for response
        await _dbContext.Entry(classEntity).Reference(c => c.Grade).LoadAsync();

        return classEntity.ToResponse();
    }

    public async Task<ClassResponse> UpdateClassAsync(int id, UpdateClassRequest request)
    {
        var classEntity = await _dbContext.StudentClasses
            .Where(c => c.Id == id && c.DeletedAt == null)
            .Include(c => c.Grade)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {id} not found");

        // Check if grade exists
        var grade = await _dbContext.StudentGrades
            .Where(g => g.Id == request.GradeId && g.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Grade with ID {request.GradeId} not found");

        // Check if code is being changed and if it's already taken
        if (classEntity.Code != request.Code && await _dbContext.StudentClasses.AnyAsync(c => c.Code == request.Code && c.DeletedAt == null && c.Id != id))
        {
            throw new ConflictException($"Class with code '{request.Code}' already exists");
        }

        classEntity.Name = request.Name;
        classEntity.Code = request.Code;
        classEntity.GradeId = request.GradeId;
        classEntity.Section = request.Section;
        classEntity.Description = request.Description;
        classEntity.Capacity = request.Capacity;
        classEntity.FeeAmount = request.FeeAmount;
        classEntity.IsActive = request.IsActive;
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return classEntity.ToResponse();
    }

    public async Task DeleteClassAsync(int id)
    {
        var classEntity = await _dbContext.StudentClasses
            .Where(c => c.Id == id && c.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {id} not found");

        // Check if there are any students in this class
        var hasStudents = await _dbContext.Students.AnyAsync(s => s.ClassId == id && s.DeletedAt == null);
        if (hasStudents)
        {
            throw new ConflictException("Cannot delete class that has enrolled students");
        }

        classEntity.DeletedAt = DateTime.UtcNow;
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task AssignStudentAsync(int classId, int studentId)
    {
        var classEntity = await _dbContext.StudentClasses
            .Where(c => c.Id == classId && c.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {classId} not found");

        var student = await _dbContext.Students
            .Where(s => s.Id == studentId && s.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {studentId} not found");

        // Check if student is already in another class
        if (student.ClassId.HasValue && student.ClassId.Value != classId)
        {
            throw new ConflictException("Student is already assigned to another class");
        }

        // Check class capacity
        var currentStudentCount = await _dbContext.Students.CountAsync(s => s.ClassId == classId && s.DeletedAt == null);
        if (currentStudentCount >= classEntity.Capacity)
        {
            throw new BusinessRuleException("Class has reached its capacity");
        }

        student.ClassId = classId;
        student.GradeId = classEntity.GradeId;
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveStudentAsync(int classId, int studentId)
    {
        var classEntity = await _dbContext.StudentClasses
            .Where(c => c.Id == classId && c.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {classId} not found");

        var student = await _dbContext.Students
            .Where(s => s.Id == studentId && s.DeletedAt == null && s.ClassId == classId)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {studentId} not found in this class");

        student.ClassId = null;
        student.GradeId = null;
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}

public static class ClassMappings
{
    public static ClassResponse ToResponse(this StudentClass classEntity)
    {
        return new ClassResponse
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Code = classEntity.Code,
            GradeId = classEntity.GradeId,
            GradeName = classEntity.Grade?.Name ?? string.Empty,
            Section = classEntity.Section,
            Description = classEntity.Description,
            Capacity = classEntity.Capacity,
            FeeAmount = classEntity.FeeAmount,
            IsActive = classEntity.IsActive,
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }

    public static ClassDetailResponse ToDetailResponse(this StudentClass classEntity)
    {
        return new ClassDetailResponse
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Code = classEntity.Code,
            GradeId = classEntity.GradeId,
            GradeName = classEntity.Grade?.Name ?? string.Empty,
            Section = classEntity.Section,
            Description = classEntity.Description,
            Capacity = classEntity.Capacity,
            FeeAmount = classEntity.FeeAmount,
            IsActive = classEntity.IsActive,
            Students = classEntity.Students?.Select(s => new StudentInClassResponse
            {
                Id = s.Id,
                StudentId = s.StudentId,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone
            }).ToList() ?? [],
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }
}
