using SchoolAccounting.Api.Features.Ledgers;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Tests.SmokeTests;

public class LedgerServiceSmokeTests : TestBase
{
    private readonly LedgerService _ledgerService;

    public LedgerServiceSmokeTests()
    {
        _ledgerService = new LedgerService(DbContext);
    }

    [Fact]
    public async Task CreateLedger_ShouldSucceed()
    {
        var request = new CreateLedgerRequest
        {
            Code = "1010",
            Name = "Cash in Bank",
            Type = "asset",
            IsActive = true
        };

        var result = await _ledgerService.CreateLedgerAsync(request);

        Assert.NotNull(result);
        Assert.Equal("1010", result.Code);
        Assert.Equal("asset", result.Type);
        Assert.Equal(0, result.Balance);
    }

    [Fact]
    public async Task CreateLedger_InvalidType_ShouldThrowBusinessRule()
    {
        var request = new CreateLedgerRequest { Code = "9999", Name = "Bad Ledger", Type = "invalid", IsActive = true };

        await Assert.ThrowsAsync<Common.Exceptions.BusinessRuleException>(
            () => _ledgerService.CreateLedgerAsync(request));
    }

    [Fact]
    public async Task CreateLedger_DuplicateCode_ShouldThrowConflict()
    {
        await _ledgerService.CreateLedgerAsync(new CreateLedgerRequest { Code = "1010", Name = "Cash", Type = "asset", IsActive = true });

        await Assert.ThrowsAsync<Common.Exceptions.ConflictException>(
            () => _ledgerService.CreateLedgerAsync(new CreateLedgerRequest { Code = "1010", Name = "Other", Type = "asset", IsActive = true }));
    }

    [Fact]
    public async Task DeleteLedger_WithNonZeroBalance_ShouldThrowConflict()
    {
        var ledger = new Ledger
        {
            Code = "5010",
            Name = "Utilities",
            Type = "expense",
            Balance = 500,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Ledgers.Add(ledger);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<Common.Exceptions.ConflictException>(
            () => _ledgerService.DeleteLedgerAsync(ledger.Id));
    }

    [Fact]
    public async Task GetLedgers_ShouldReturnPagedResults()
    {
        DbContext.Ledgers.AddRange(
            new Ledger { Code = "1000", Name = "Cash", Type = "asset", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "4000", Name = "Revenue", Type = "revenue", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5000", Name = "Expense", Type = "expense", Balance = 0, IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await DbContext.SaveChangesAsync();

        var assets = await _ledgerService.GetLedgersAsync(type: "asset");
        var all = await _ledgerService.GetLedgersAsync();

        Assert.Equal(1, assets.Meta.Total);
        Assert.Equal(3, all.Meta.Total);
    }
}
