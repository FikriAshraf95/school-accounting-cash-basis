using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Categories;

public class CategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CategoryResponse>> GetCategoriesAsync(string? type, bool? isActive)
    {
        var query = _dbContext.Categories
            .Include(c => c.Ledger)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(c => c.Type == type.ToLower());
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var total = await query.CountAsync();

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return new PagedResult<CategoryResponse>
        {
            Data = categories.Select(c => c.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = 1,
                PerPage = total,
                LastPage = 1
            }
        };
    }

    public async Task<CategoryResponse> GetCategoryAsync(int id)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Ledger)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Category with ID {id} not found");

        return category.ToResponse();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // Validate type
        if (!IsValidCategoryType(request.Type))
        {
            throw new BusinessRuleException($"Invalid category type: {request.Type}. Must be 'income' or 'expense'");
        }

        // Check if ledger exists
        var ledger = await _dbContext.Ledgers
            .Where(l => l.Id == request.LedgerId && l.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Ledger with ID {request.LedgerId} not found");

        var category = new Category
        {
            Name = request.Name,
            Type = request.Type.ToLower(),
            LedgerId = request.LedgerId,
            Description = request.Description,
            RequiresStudent = request.RequiresStudent,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        // Reload with ledger for response
        await _dbContext.Entry(category).Reference(c => c.Ledger).LoadAsync();

        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Ledger)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Category with ID {id} not found");

        // Check if ledger exists
        var ledger = await _dbContext.Ledgers
            .Where(l => l.Id == request.LedgerId && l.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Ledger with ID {request.LedgerId} not found");

        category.Name = request.Name;
        category.LedgerId = request.LedgerId;
        category.Description = request.Description;
        category.RequiresStudent = request.RequiresStudent;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return category.ToResponse();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Category with ID {id} not found");

        // Check if category is used in any transaction items
        // This would be checked when TransactionItems feature is implemented
        // For now, we just allow deletion

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
    }

    private static bool IsValidCategoryType(string type)
    {
        var validTypes = new[] { "income", "expense" };
        return validTypes.Contains(type.ToLower());
    }
}

public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            LedgerId = category.LedgerId,
            LedgerName = category.Ledger?.Name ?? string.Empty,
            LedgerCode = category.Ledger?.Code ?? string.Empty,
            Description = category.Description,
            RequiresStudent = category.RequiresStudent,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
