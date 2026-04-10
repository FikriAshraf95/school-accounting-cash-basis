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

    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(int? year)
    {
        var query = _dbContext.Ledgers
            .Where(l => l.DeletedAt == null && l.IsActive)
            .AsQueryable();

        // Get all journal entries for the specified period
        var journalEntriesQuery = _dbContext.JournalEntries
            .Where(j => j.Ledger.DeletedAt == null)
            .AsQueryable();

        if (year.HasValue)
        {
            var startDate = new DateTime(year.Value, 1, 1);
            var endDate = new DateTime(year.Value, 12, 31);
            journalEntriesQuery = journalEntriesQuery.Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate);
        }

        var journalEntries = await journalEntriesQuery
            .GroupBy(j => j.LedgerId)
            .Select(g => new
            {
                LedgerId = g.Key,
                TotalDebits = g.Where(j => j.EntryType == "debit").Sum(j => j.Amount),
                TotalCredits = g.Where(j => j.EntryType == "credit").Sum(j => j.Amount)
            })
            .ToListAsync();

        var ledgers = await query
            .OrderBy(l => l.Code)
            .ToListAsync();

        var items = new List<TrialBalanceItemResponse>();
        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var ledger in ledgers)
        {
            var entrySummary = journalEntries.FirstOrDefault(j => j.LedgerId == ledger.Id);
            var debits = entrySummary?.TotalDebits ?? 0;
            var credits = entrySummary?.TotalCredits ?? 0;

            // For debit-normal accounts (asset, expense), positive balance = debit
            // For credit-normal accounts (liability, equity, revenue), positive balance = credit
            var isDebitNormal = ledger.Type == "asset" || ledger.Type == "expense";

            decimal debitAmount;
            decimal creditAmount;

            if (isDebitNormal)
            {
                var netBalance = debits - credits;
                if (netBalance > 0)
                {
                    debitAmount = netBalance;
                    creditAmount = 0;
                }
                else
                {
                    debitAmount = 0;
                    creditAmount = Math.Abs(netBalance);
                }
            }
            else
            {
                var netBalance = credits - debits;
                if (netBalance > 0)
                {
                    debitAmount = 0;
                    creditAmount = netBalance;
                }
                else
                {
                    debitAmount = Math.Abs(netBalance);
                    creditAmount = 0;
                }
            }

            // Only include ledgers that have activity or a balance
            if (debitAmount != 0 || creditAmount != 0 || ledger.Balance != 0)
            {
                items.Add(new TrialBalanceItemResponse
                {
                    LedgerId = ledger.Id,
                    LedgerCode = ledger.Code,
                    LedgerName = ledger.Name,
                    Type = ledger.Type,
                    DebitAmount = debitAmount,
                    CreditAmount = creditAmount
                });

                totalDebits += debitAmount;
                totalCredits += creditAmount;
            }
        }

        return new TrialBalanceResponse
        {
            Items = items,
            TotalDebits = totalDebits,
            TotalCredits = totalCredits,
            IsBalanced = totalDebits == totalCredits
        };
    }

    public async Task<LedgerSummaryResponse> GetLedgerSummaryByYearAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        // Get previous year closing balances for opening balance calculation
        var prevYear = year - 1;
        var hasPriorYearClosing = await _dbContext.ClosedYears.AnyAsync(cy => cy.Year == prevYear);

        var ledgers = await _dbContext.Ledgers
            .Where(l => l.DeletedAt == null)
            .OrderBy(l => l.Code)
            .ToListAsync();

        var journalEntries = await _dbContext.JournalEntries
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate && j.Ledger.DeletedAt == null)
            .GroupBy(j => j.LedgerId)
            .Select(g => new
            {
                LedgerId = g.Key,
                TotalDebits = g.Where(j => j.EntryType == "debit").Sum(j => j.Amount),
                TotalCredits = g.Where(j => j.EntryType == "credit").Sum(j => j.Amount)
            })
            .ToListAsync();

        // Get opening balance entries for this year (if any)
        var openingEntries = await _dbContext.JournalEntries
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate
                && j.JournalType == "opening"
                && j.Ledger.DeletedAt == null)
            .GroupBy(j => j.LedgerId)
            .Select(g => new
            {
                LedgerId = g.Key,
                OpeningDebits = g.Where(j => j.EntryType == "debit").Sum(j => j.Amount),
                OpeningCredits = g.Where(j => j.EntryType == "credit").Sum(j => j.Amount)
            })
            .ToListAsync();

        var items = new List<LedgerSummaryItemResponse>();
        decimal totalAssets = 0;
        decimal totalLiabilities = 0;
        decimal totalEquity = 0;
        decimal totalRevenue = 0;
        decimal totalExpenses = 0;

        foreach (var ledger in ledgers)
        {
            var entrySummary = journalEntries.FirstOrDefault(j => j.LedgerId == ledger.Id);
            var openingEntry = openingEntries.FirstOrDefault(o => o.LedgerId == ledger.Id);

            var totalDebits = entrySummary?.TotalDebits ?? 0;
            var totalCredits = entrySummary?.TotalCredits ?? 0;

            // Calculate opening balance
            decimal openingBalance = 0;
            if (openingEntry != null)
            {
                var isDebitNormal = ledger.Type == "asset" || ledger.Type == "expense";
                if (isDebitNormal)
                {
                    openingBalance = openingEntry.OpeningDebits - openingEntry.OpeningCredits;
                }
                else
                {
                    openingBalance = openingEntry.OpeningCredits - openingEntry.OpeningDebits;
                }
            }
            else if (!hasPriorYearClosing && ledger.Type is "asset" or "liability" or "equity")
            {
                // If no prior year closing, assume current ledger balance is the opening
                // This is a simplification for the first year of operation
                openingBalance = ledger.Balance;
            }

            // Calculate net change and closing balance
            var isDebitNormalAccount = ledger.Type == "asset" || ledger.Type == "expense";
            decimal netChange;
            decimal closingBalance;

            if (isDebitNormalAccount)
            {
                netChange = totalDebits - totalCredits;
                closingBalance = openingBalance + netChange;
            }
            else
            {
                netChange = totalCredits - totalDebits;
                closingBalance = openingBalance + netChange;
            }

            items.Add(new LedgerSummaryItemResponse
            {
                LedgerId = ledger.Id,
                LedgerCode = ledger.Code,
                LedgerName = ledger.Name,
                Type = ledger.Type,
                OpeningBalance = openingBalance,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                NetChange = netChange,
                ClosingBalance = closingBalance
            });

            // Accumulate totals by type
            switch (ledger.Type)
            {
                case "asset":
                    totalAssets += closingBalance;
                    break;
                case "liability":
                    totalLiabilities += closingBalance;
                    break;
                case "equity":
                    totalEquity += closingBalance;
                    break;
                case "revenue":
                    totalRevenue += closingBalance;
                    break;
                case "expense":
                    totalExpenses += closingBalance;
                    break;
            }
        }

        return new LedgerSummaryResponse
        {
            Year = year,
            Ledgers = items,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses
        };
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
