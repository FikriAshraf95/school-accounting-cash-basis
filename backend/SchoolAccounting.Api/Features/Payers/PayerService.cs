using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Payers;

public class PayerService
{
    private readonly AppDbContext _dbContext;

    public PayerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PayerResponse>> GetPayersAsync(
        int page = 1,
        int perPage = 15,
        string? sortBy = null,
        bool sortDesc = false,
        string? search = null,
        string? type = null,
        bool? isActive = null)
    {
        var query = _dbContext.Payers
            .Where(p => p.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.PayerCode.Contains(search));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(p => p.Type == type.ToLower());
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "payercode" => sortDesc ? query.OrderByDescending(p => p.PayerCode) : query.OrderBy(p => p.PayerCode),
            "balance" => sortDesc ? query.OrderByDescending(p => p.Balance) : query.OrderBy(p => p.Balance),
            _ => sortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync();

        var payers = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResult<PayerResponse>
        {
            Data = payers.Select(p => p.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage > 0 ? lastPage : 1
            }
        };
    }

    public async Task<PayerDetailResponse> GetPayerAsync(int id)
    {
        var payer = await _dbContext.Payers
            .Where(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Payer with ID {id} not found");

        var response = payer.ToDetailResponse();

        // Load transaction history
        var transactions = await _dbContext.Transactions
            .Where(t => t.PayerId == id && t.DeletedAt == null)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new PayerTransactionSummaryResponse
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

    public async Task<PayerResponse> CreatePayerAsync(CreatePayerRequest request)
    {
        // Validate type
        if (!IsValidPayerType(request.Type))
        {
            throw new BusinessRuleException($"Invalid payer type: {request.Type}. Must be one of: donor, sponsor, vendor, supplier, general, government");
        }

        // Validate category
        if (!IsValidPayerCategory(request.Category))
        {
            throw new BusinessRuleException($"Invalid payer category: {request.Category}. Must be one of: individual, corporate, government, ngo");
        }

        // Check if payer code already exists
        if (await _dbContext.Payers.AnyAsync(p => p.PayerCode == request.PayerCode && p.DeletedAt == null))
        {
            throw new ConflictException($"Payer with code '{request.PayerCode}' already exists");
        }

        var payer = new Payer
        {
            PayerCode = request.PayerCode,
            Name = request.Name,
            Type = request.Type.ToLower(),
            Category = request.Category.ToLower(),
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Balance = 0,
            IsRecurring = request.IsRecurring,
            Notes = request.Notes,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Payers.Add(payer);
        await _dbContext.SaveChangesAsync();

        return payer.ToResponse();
    }

    public async Task<PayerResponse> UpdatePayerAsync(int id, UpdatePayerRequest request)
    {
        var payer = await _dbContext.Payers
            .Where(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Payer with ID {id} not found");

        // Validate type
        if (!IsValidPayerType(request.Type))
        {
            throw new BusinessRuleException($"Invalid payer type: {request.Type}. Must be one of: donor, sponsor, vendor, supplier, general, government");
        }

        // Validate category
        if (!IsValidPayerCategory(request.Category))
        {
            throw new BusinessRuleException($"Invalid payer category: {request.Category}. Must be one of: individual, corporate, government, ngo");
        }

        // Check if payer code is being changed and if it's already taken
        if (payer.PayerCode != request.PayerCode && await _dbContext.Payers.AnyAsync(p => p.PayerCode == request.PayerCode && p.DeletedAt == null && p.Id != id))
        {
            throw new ConflictException($"Payer with code '{request.PayerCode}' already exists");
        }

        payer.PayerCode = request.PayerCode;
        payer.Name = request.Name;
        payer.Type = request.Type.ToLower();
        payer.Category = request.Category.ToLower();
        payer.Email = request.Email;
        payer.Phone = request.Phone;
        payer.Address = request.Address;
        payer.IsRecurring = request.IsRecurring;
        payer.Notes = request.Notes;
        payer.IsActive = request.IsActive;
        payer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return payer.ToResponse();
    }

    public async Task DeletePayerAsync(int id)
    {
        var payer = await _dbContext.Payers
            .Where(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Payer with ID {id} not found");

        // Check if payer has transactions
        var hasTransactions = await _dbContext.Transactions.AnyAsync(t => t.PayerId == id && t.DeletedAt == null);
        if (hasTransactions)
        {
            throw new ConflictException("Cannot delete payer with transaction history");
        }

        payer.DeletedAt = DateTime.UtcNow;
        payer.UpdatedAt = DateTime.UtcNow;
        payer.IsActive = false;

        await _dbContext.SaveChangesAsync();
    }

    private static bool IsValidPayerType(string type)
    {
        var validTypes = new[] { "donor", "sponsor", "vendor", "supplier", "general", "government" };
        return validTypes.Contains(type.ToLower());
    }

    private static bool IsValidPayerCategory(string category)
    {
        var validCategories = new[] { "individual", "corporate", "government", "ngo" };
        return validCategories.Contains(category.ToLower());
    }
}

public static class PayerMappings
{
    public static PayerResponse ToResponse(this Payer payer)
    {
        return new PayerResponse
        {
            Id = payer.Id,
            PayerCode = payer.PayerCode,
            Name = payer.Name,
            Type = payer.Type,
            Category = payer.Category,
            Email = payer.Email,
            Phone = payer.Phone,
            Address = payer.Address,
            Balance = payer.Balance,
            IsRecurring = payer.IsRecurring,
            Notes = payer.Notes,
            IsActive = payer.IsActive,
            DeletedAt = payer.DeletedAt,
            CreatedAt = payer.CreatedAt,
            UpdatedAt = payer.UpdatedAt
        };
    }

    public static PayerDetailResponse ToDetailResponse(this Payer payer)
    {
        return new PayerDetailResponse
        {
            Id = payer.Id,
            PayerCode = payer.PayerCode,
            Name = payer.Name,
            Type = payer.Type,
            Category = payer.Category,
            Email = payer.Email,
            Phone = payer.Phone,
            Address = payer.Address,
            Balance = payer.Balance,
            IsRecurring = payer.IsRecurring,
            Notes = payer.Notes,
            IsActive = payer.IsActive,
            DeletedAt = payer.DeletedAt,
            CreatedAt = payer.CreatedAt,
            UpdatedAt = payer.UpdatedAt,
            TransactionHistory = []
        };
    }
}
