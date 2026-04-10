using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.JournalEntries;

public class JournalEntryService
{
    private readonly AppDbContext _dbContext;

    public JournalEntryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<JournalEntryResponse>> GetJournalEntriesAsync(
        int page = 1,
        int perPage = 15,
        int? ledgerId = null,
        string? journalType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _dbContext.JournalEntries
            .Include(j => j.Ledger)
            .Include(j => j.Transaction)
            .Include(j => j.CreatedByUser)
            .AsQueryable();

        if (ledgerId.HasValue)
        {
            query = query.Where(j => j.LedgerId == ledgerId.Value);
        }

        if (!string.IsNullOrEmpty(journalType))
        {
            query = query.Where(j => j.JournalType == journalType.ToLower());
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(j => j.EntryDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(j => j.EntryDate <= dateTo.Value);
        }

        var total = await query.CountAsync();

        var entries = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResult<JournalEntryResponse>
        {
            Data = entries.Select(j => j.ToResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage > 0 ? lastPage : 1
            }
        };
    }
}

public class JournalEntryResponse
{
    public int Id { get; set; }
    public int? TransactionId { get; set; }
    public string? TransactionNumber { get; set; }
    public int LedgerId { get; set; }
    public string? LedgerCode { get; set; }
    public string? LedgerName { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public string JournalType { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class JournalEntryMappings
{
    public static JournalEntryResponse ToResponse(this JournalEntry entry)
    {
        return new JournalEntryResponse
        {
            Id = entry.Id,
            TransactionId = entry.TransactionId,
            TransactionNumber = entry.Transaction?.TransactionNumber,
            LedgerId = entry.LedgerId,
            LedgerCode = entry.Ledger?.Code,
            LedgerName = entry.Ledger?.Name,
            EntryType = entry.EntryType,
            Amount = entry.Amount,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            JournalType = entry.JournalType,
            CreatedBy = entry.CreatedBy,
            CreatedByName = entry.CreatedByUser?.Name,
            CreatedAt = entry.CreatedAt
        };
    }
}
