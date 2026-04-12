using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Students;

public class StudentService
{
    private readonly AppDbContext _dbContext;

    public StudentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<StudentResponse>> GetStudentsAsync(
        int page = 1,
        int perPage = 10,
        string? sortBy = null,
        bool sortDesc = false,
        string? search = null,
        int? classId = null,
        int? gradeId = null,
        bool? isActive = null)
    {
        var query = _dbContext.Students
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Class)
            .Include(s => s.Grade)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.Name.Contains(search) || s.StudentId.Contains(search));
        }

        if (classId.HasValue)
        {
            query = query.Where(s => s.ClassId == classId.Value);
        }

        if (gradeId.HasValue)
        {
            query = query.Where(s => s.GradeId == gradeId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "studentid" => sortDesc ? query.OrderByDescending(s => s.StudentId) : query.OrderBy(s => s.StudentId),
            "balance" => sortDesc ? query.OrderByDescending(s => s.Balance) : query.OrderBy(s => s.Balance),
            _ => sortDesc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        var total = await query.CountAsync();

        var students = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResult<StudentResponse>
        {
            Data = students.Select(s => s.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage > 0 ? lastPage : 1
            }
        };
    }

    public async Task<StudentDetailResponse> GetStudentAsync(int id)
    {
        var student = await _dbContext.Students
            .Where(s => s.Id == id && s.DeletedAt == null)
            .Include(s => s.Class)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {id} not found");

        var response = student.ToDetailResponse();

        // Load transaction history
        var transactions = await _dbContext.Transactions
            .Where(t => t.StudentId == id && t.DeletedAt == null)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new StudentTransactionSummaryResponse
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                Type = t.Type,
                Amount = t.Amount,
                Description = t.Description,
                ReceiptNumber = t.ReceiptNumber,
                IsReversed = t.IsReversed,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        response.TransactionHistory = transactions;
        return response;
    }

    public async Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request)
    {
        // Check if student code already exists
        if (await _dbContext.Students.AnyAsync(s => s.StudentId == request.StudentId && s.DeletedAt == null))
        {
            throw new ConflictException($"Student with ID '{request.StudentId}' already exists");
        }

        // Validate class if provided
        if (request.ClassId.HasValue)
        {
            var classExists = await _dbContext.StudentClasses.AnyAsync(c => c.Id == request.ClassId.Value && c.DeletedAt == null);
            if (!classExists)
            {
                throw new NotFoundException($"Class with ID {request.ClassId.Value} not found");
            }
        }

        // Validate grade if provided
        if (request.GradeId.HasValue)
        {
            var gradeExists = await _dbContext.StudentGrades.AnyAsync(g => g.Id == request.GradeId.Value && g.DeletedAt == null);
            if (!gradeExists)
            {
                throw new NotFoundException($"Grade with ID {request.GradeId.Value} not found");
            }
        }

        var student = new Student
        {
            StudentId = request.StudentId,
            Name = request.Name,
            ClassId = request.ClassId,
            GradeId = request.GradeId,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Balance = 0,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();

        // Reload with includes
        await _dbContext.Entry(student)
            .Reference(s => s.Class)
            .LoadAsync();
        await _dbContext.Entry(student)
            .Reference(s => s.Grade)
            .LoadAsync();

        return student.ToResponse();
    }

    public async Task<StudentResponse> UpdateStudentAsync(int id, UpdateStudentRequest request)
    {
        var student = await _dbContext.Students
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {id} not found");

        // Check if student code is being changed and if it's already taken
        if (student.StudentId != request.StudentId && await _dbContext.Students.AnyAsync(s => s.StudentId == request.StudentId && s.DeletedAt == null && s.Id != id))
        {
            throw new ConflictException($"Student with ID '{request.StudentId}' already exists");
        }

        // Validate class if provided
        if (request.ClassId.HasValue)
        {
            var classExists = await _dbContext.StudentClasses.AnyAsync(c => c.Id == request.ClassId.Value && c.DeletedAt == null);
            if (!classExists)
            {
                throw new NotFoundException($"Class with ID {request.ClassId.Value} not found");
            }
        }

        // Validate grade if provided
        if (request.GradeId.HasValue)
        {
            var gradeExists = await _dbContext.StudentGrades.AnyAsync(g => g.Id == request.GradeId.Value && g.DeletedAt == null);
            if (!gradeExists)
            {
                throw new NotFoundException($"Grade with ID {request.GradeId.Value} not found");
            }
        }

        student.StudentId = request.StudentId;
        student.Name = request.Name;
        student.ClassId = request.ClassId;
        student.GradeId = request.GradeId;
        student.Email = request.Email;
        student.Phone = request.Phone;
        student.Address = request.Address;
        student.IsActive = request.IsActive;
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Reload with includes
        await _dbContext.Entry(student)
            .Reference(s => s.Class)
            .LoadAsync();
        await _dbContext.Entry(student)
            .Reference(s => s.Grade)
            .LoadAsync();

        return student.ToResponse();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _dbContext.Students
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {id} not found");

        // Check if student has transactions
        var hasTransactions = await _dbContext.Transactions.AnyAsync(t => t.StudentId == id && t.DeletedAt == null);
        if (hasTransactions)
        {
            throw new ConflictException("Cannot delete student with transaction history");
        }

        student.DeletedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        student.IsActive = false;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<StudentResponse> AssignToClassAsync(int studentId, int classId)
    {
        var student = await _dbContext.Students
            .Where(s => s.Id == studentId && s.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Student with ID {studentId} not found");

        var studentClass = await _dbContext.StudentClasses
            .Where(c => c.Id == classId && c.DeletedAt == null)
            .Include(c => c.Grade)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Class with ID {classId} not found");

        student.ClassId = classId;
        student.GradeId = studentClass.GradeId;
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return student.ToResponse();
    }

    public async Task<List<StudentReportResponse>> GetStudentReportAsync(StudentReportRequest request)
    {
        var query = _dbContext.Students
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Class)
            .Include(s => s.Grade)
            .AsQueryable();

        if (request.GradeId.HasValue)
        {
            query = query.Where(s => s.GradeId == request.GradeId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(s => s.ClassId == request.ClassId.Value);
        }

        var students = await query.ToListAsync();

        // Get transaction summaries
        var studentIds = students.Select(s => s.Id).ToList();

        var transactionSummaries = await _dbContext.Transactions
            .Where(t => t.StudentId != null && studentIds.Contains(t.StudentId.Value) && t.DeletedAt == null)
            .GroupBy(t => t.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                TotalIncome = g.Where(t => t.Type == "income").Sum(t => t.Amount),
                TotalExpense = g.Where(t => t.Type == "expense").Sum(t => t.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var result = students.Select(s =>
        {
            var summary = transactionSummaries.FirstOrDefault(ts => ts.StudentId == s.Id);
            return new StudentReportResponse
            {
                Id = s.Id,
                StudentId = s.StudentId,
                Name = s.Name,
                ClassName = s.Class?.Name,
                GradeName = s.Grade?.Name,
                Balance = s.Balance,
                TotalIncome = summary?.TotalIncome ?? 0,
                TotalExpense = summary?.TotalExpense ?? 0,
                TransactionCount = summary?.Count ?? 0
            };
        }).ToList();

        return result;
    }

    public async Task<StudentImportResult> ImportStudentsAsync(List<CreateStudentRequest> requests)
    {
        var result = new StudentImportResult();

        foreach (var request in requests)
        {
            try
            {
                // Check if student code already exists
                if (await _dbContext.Students.AnyAsync(s => s.StudentId == request.StudentId && s.DeletedAt == null))
                {
                    result.Skipped++;
                    result.Errors.Add($"Student with ID '{request.StudentId}' already exists");
                    continue;
                }

                // Validate class if provided
                if (request.ClassId.HasValue)
                {
                    var classExists = await _dbContext.StudentClasses.AnyAsync(c => c.Id == request.ClassId.Value && c.DeletedAt == null);
                    if (!classExists)
                    {
                        result.Skipped++;
                        result.Errors.Add($"Class with ID {request.ClassId.Value} not found for student {request.StudentId}");
                        continue;
                    }
                }

                // Validate grade if provided
                if (request.GradeId.HasValue)
                {
                    var gradeExists = await _dbContext.StudentGrades.AnyAsync(g => g.Id == request.GradeId.Value && g.DeletedAt == null);
                    if (!gradeExists)
                    {
                        result.Skipped++;
                        result.Errors.Add($"Grade with ID {request.GradeId.Value} not found for student {request.StudentId}");
                        continue;
                    }
                }

                var student = new Student
                {
                    StudentId = request.StudentId,
                    Name = request.Name,
                    ClassId = request.ClassId,
                    GradeId = request.GradeId,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    Balance = 0,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Students.Add(student);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Skipped++;
                result.Errors.Add($"Error importing student {request.StudentId}: {ex.Message}");
            }
        }

        await _dbContext.SaveChangesAsync();
        return result;
    }
}

public static class StudentMappings
{
    public static StudentResponse ToResponse(this Student student)
    {
        return new StudentResponse
        {
            Id = student.Id,
            StudentId = student.StudentId,
            Name = student.Name,
            ClassId = student.ClassId,
            ClassName = student.Class?.Name,
            GradeId = student.GradeId,
            GradeName = student.Grade?.Name,
            Email = student.Email,
            Phone = student.Phone,
            Address = student.Address,
            Balance = student.Balance,
            IsActive = student.IsActive,
            DeletedAt = student.DeletedAt,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }

    public static StudentDetailResponse ToDetailResponse(this Student student)
    {
        return new StudentDetailResponse
        {
            Id = student.Id,
            StudentId = student.StudentId,
            Name = student.Name,
            ClassId = student.ClassId,
            ClassName = student.Class?.Name,
            GradeId = student.GradeId,
            GradeName = student.Grade?.Name,
            Email = student.Email,
            Phone = student.Phone,
            Address = student.Address,
            Balance = student.Balance,
            IsActive = student.IsActive,
            DeletedAt = student.DeletedAt,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            TransactionHistory = []
        };
    }
}
