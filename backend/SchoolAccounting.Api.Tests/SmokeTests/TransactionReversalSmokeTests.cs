using SchoolAccounting.Api.Features.Transactions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Tests.SmokeTests;

public class TransactionReversalSmokeTests : TestBase
{
    private readonly TransactionService _transactionService;
    private readonly TransactionReversalService _reversalService;
    private int _testUserId;
    private int _cashLedgerId;
    private int _studentId;
    private int _incomeCategoryId;

    public TransactionReversalSmokeTests()
    {
        _transactionService = new TransactionService(DbContext);
        _reversalService = new TransactionReversalService(DbContext);
    }

    private async Task SetupTestDataAsync()
    {
        // Create test user
        var user = new User
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        _testUserId = user.Id;

        // Create cash ledger
        var cashLedger = new Ledger
        {
            Code = "1010",
            Name = "Cash in Bank",
            Type = "asset",
            Balance = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Ledgers.Add(cashLedger);
        await DbContext.SaveChangesAsync();
        _cashLedgerId = cashLedger.Id;

        // Create revenue ledger
        var revenueLedger = new Ledger
        {
            Code = "4000",
            Name = "Tuition Fee Income",
            Type = "revenue",
            Balance = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Ledgers.Add(revenueLedger);
        await DbContext.SaveChangesAsync();

        // Create income category
        var incomeCategory = new Category
        {
            Name = "Tuition Fee",
            Type = "income",
            LedgerId = revenueLedger.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Categories.Add(incomeCategory);
        await DbContext.SaveChangesAsync();
        _incomeCategoryId = incomeCategory.Id;

        // Create test student
        var student = new Student
        {
            StudentId = "STU001",
            Name = "Test Student",
            Balance = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Students.Add(student);
        await DbContext.SaveChangesAsync();
        _studentId = student.Id;
    }

    private async Task<TransactionResponse> CreateTestTransactionAsync()
    {
        var request = new CreateTransactionRequest
        {
            TransactionDate = DateTime.UtcNow.AddDays(-1), // Yesterday's transaction
            Type = "income",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
            PaymentMethod = "cash",
            Description = "Test payment",
            Items = new List<CreateTransactionItemRequest>
            {
                new()
                {
                    CategoryId = _incomeCategoryId,
                    Amount = 500,
                    Quantity = 1,
                    UnitPrice = 500
                }
            }
        };
        return await _transactionService.CreateTransactionAsync(request, _testUserId);
    }

    [Fact]
    public async Task ReverseTransaction_ShouldCreateReversalTransaction_AndReverseBalances()
    {
        // Arrange
        await SetupTestDataAsync();
        var originalTransaction = await CreateTestTransactionAsync();
        var originalCashLedger = await DbContext.Ledgers.FindAsync(_cashLedgerId);
        var originalBalance = originalCashLedger!.Balance;

        // Act
        var reversal = await _reversalService.ReverseTransactionAsync(originalTransaction.Id, _testUserId);

        // Assert
        Assert.NotNull(reversal);
        Assert.NotEqual(originalTransaction.Id, reversal.Id);
        Assert.Equal("expense", reversal.Type); // Reversal has opposite type
        Assert.True(reversal.Amount == originalTransaction.Amount);
        Assert.Contains("REVERSAL", reversal.ReferenceNumber);

        // Verify original transaction is marked as reversed
        var updatedOriginal = await DbContext.Transactions.FindAsync(originalTransaction.Id);
        Assert.True(updatedOriginal!.IsReversed);
        Assert.Equal(reversal.Id, updatedOriginal.ReversalTransactionId);

        // Verify balances were reversed
        var updatedCashLedger = await DbContext.Ledgers.FindAsync(_cashLedgerId);
        Assert.Equal(0, updatedCashLedger!.Balance); // Balance should be back to zero

        // Verify student balance was reversed
        var student = await DbContext.Students.FindAsync(_studentId);
        Assert.Equal(0, student!.Balance);

        // Verify reversal journal entries exist
        var reversalJournalEntries = DbContext.JournalEntries.Where(j => j.TransactionId == reversal.Id).ToList();
        Assert.Equal(2, reversalJournalEntries.Count);
        Assert.All(reversalJournalEntries, j => Assert.Equal("reversal", j.JournalType));
    }

    [Fact]
    public async Task ReverseAlreadyReversedTransaction_ShouldThrowBusinessRuleException()
    {
        // Arrange
        await SetupTestDataAsync();
        var originalTransaction = await CreateTestTransactionAsync();
        await _reversalService.ReverseTransactionAsync(originalTransaction.Id, _testUserId);

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.BusinessRuleException>(
            () => _reversalService.ReverseTransactionAsync(originalTransaction.Id, _testUserId));
    }

    [Fact]
    public async Task ReverseNonExistentTransaction_ShouldThrowNotFoundException()
    {
        // Arrange
        await SetupTestDataAsync();

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.NotFoundException>(
            () => _reversalService.ReverseTransactionAsync(9999, _testUserId));
    }

    [Fact]
    public async Task ReverseTransaction_WhenCurrentYearClosed_ShouldThrowBusinessRuleException()
    {
        // Arrange
        await SetupTestDataAsync();
        var originalTransaction = await CreateTestTransactionAsync();

        // Close current year
        var currentYear = DateTime.UtcNow.Year;
        var closedYear = new ClosedYear
        {
            Year = currentYear,
            ClosedAt = DateTime.UtcNow,
            ClosedBy = _testUserId
        };
        DbContext.ClosedYears.Add(closedYear);
        await DbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.BusinessRuleException>(
            () => _reversalService.ReverseTransactionAsync(originalTransaction.Id, _testUserId));
    }
}
