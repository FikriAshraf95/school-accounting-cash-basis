using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Ledgers;

public class LedgerService
{
    private readonly AppDbContext _dbContext;

    public LedgerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<LedgerResponse>> GetLedgersAsync(string? type, bool? isActive)
    {
        var query = _dbContext.Ledgers
            .Where(l => l.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(l => l.Type == type.ToLower());
        }

        if (isActive.HasValue)
        {
            query = query.Where(l => l.IsActive == isActive.Value);
        }

        var total = await query.CountAsync();

        var ledgers = await query
            .OrderBy(l => l.Code)
            .ToListAsync();

        return new PagedResult<LedgerResponse>
        {
            Data = ledgers.Select(l => l.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = 1,
                PerPage = total,
                LastPage = 1
            }
        };
    }

    public async Task<LedgerResponse> GetLedgerAsync(int id)
    {
        var ledger = await _dbContext.Ledgers
            .Where(l => l.Id == id && l.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Ledger with ID {id} not found");

        return ledger.ToResponse();
    }

    public async Task<LedgerResponse> CreateLedgerAsync(CreateLedgerRequest request)
    {
        // Validate type
        if (!IsValidLedgerType(request.Type))
        {
            throw new BusinessRuleException($"Invalid ledger type: {request.Type}. Must be one of: asset, liability, equity, revenue, expense");
        }

        // Check if code already exists
        if (await _dbContext.Ledgers.AnyAsync(l => l.Code == request.Code && l.DeletedAt == null))
        {
            throw new ConflictException($"Ledger with code '{request.Code}' already exists");
        }

        var ledger = new Ledger
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type.ToLower(),
            Category = request.Category,
            Balance = 0,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Ledgers.Add(ledger);
        await _dbContext.SaveChangesAsync();

        return ledger.ToResponse();
    }

    public async Task<LedgerResponse> UpdateLedgerAsync(int id, UpdateLedgerRequest request)
    {
        var ledger = await _dbContext.Ledgers
            .Where(l => l.Id == id && l.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Ledger with ID {id} not found");

        // Check if code is being changed and if it's already taken
        if (ledger.Code != request.Code && await _dbContext.Ledgers.AnyAsync(l => l.Code == request.Code && l.DeletedAt == null && l.Id != id))
        {
            throw new ConflictException($"Ledger with code '{request.Code}' already exists");
        }

        ledger.Name = request.Name;
        ledger.Code = request.Code;
        ledger.Category = request.Category;
        ledger.IsActive = request.IsActive;
        ledger.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ledger.ToResponse();
    }

    public async Task DeleteLedgerAsync(int id)
    {
        var ledger = await _dbContext.Ledgers
            .Where(l => l.Id == id && l.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Ledger with ID {id} not found");

        // Check if ledger is used in any categories
        var hasCategories = await _dbContext.Categories.AnyAsync(c => c.LedgerId == id);
        if (hasCategories)
        {
            throw new ConflictException("Cannot delete ledger that is linked to categories");
        }

        // Check if ledger has any journal entries
        // This would be checked when JournalEntries feature is implemented
        // For now, we just check if balance is non-zero as a simple guard
        if (ledger.Balance != 0)
        {
            throw new ConflictException("Cannot delete ledger with non-zero balance");
        }

        ledger.DeletedAt = DateTime.UtcNow;
        ledger.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private static bool IsValidLedgerType(string type)
    {
        var validTypes = new[] { "asset", "liability", "equity", "revenue", "expense" };
        return validTypes.Contains(type.ToLower());
    }
}

public static class LedgerMappings
{
    public static LedgerResponse ToResponse(this Ledger ledger)
    {
        return new LedgerResponse
        {
            Id = ledger.Id,
            Code = ledger.Code,
            Name = ledger.Name,
            Type = ledger.Type,
            Category = ledger.Category,
            Balance = ledger.Balance,
            IsActive = ledger.IsActive,
            CreatedAt = ledger.CreatedAt,
            UpdatedAt = ledger.UpdatedAt
        };
    }
}
