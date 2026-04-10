using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Categories;

public class CategoryService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "categories";

    public CategoryService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<PagedResult<CategoryResponse>> GetCategoriesAsync(string? type, bool? isActive)
    {
        // Fetch all from DB (or cache), then filter in memory.
        // Categories are reference data that changes rarely — 5-minute TTL is safe.
        if (!_cache.TryGetValue(CacheKey, out List<CategoryResponse>? allCategories))
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Ledger)
                .OrderBy(c => c.Name)
                .ToListAsync();
            allCategories = categories.Select(c => c.ToResponse()).ToList();
            _cache.Set(CacheKey, allCategories, TimeSpan.FromMinutes(5));
        }

        var query = allCategories!.AsQueryable();
        if (!string.IsNullOrEmpty(type)) query = query.Where(c => c.Type == type.ToLower());
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);
        var list = query.ToList();

        return new PagedResult<CategoryResponse>
        {
            Data = list,
            Meta = new PagedResultMeta
            {
                Total = list.Count,
                Page = 1,
                PerPage = list.Count,
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
        _cache.Remove(CacheKey);

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
        _cache.Remove(CacheKey);

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
        _cache.Remove(CacheKey);
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
