using SchoolAccounting.Api.Features.Categories;
using SchoolAccounting.Api.Features.Ledgers;
using SchoolAccounting.Api.Features.Students;
using SchoolAccounting.Api.Features.Transactions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Tests.SmokeTests;

public class TransactionSmokeTests : TestBase
{
    private readonly TransactionService _transactionService;
    private int _testUserId;
    private int _cashLedgerId;
    private int _studentId;
    private int _incomeCategoryId;
    private int _expenseCategoryId;

    public TransactionSmokeTests()
    {
        _transactionService = new TransactionService(DbContext);
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

        // Create expense ledger
        var expenseLedger = new Ledger
        {
            Code = "5000",
            Name = "Salaries Expense",
            Type = "expense",
            Balance = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Ledgers.Add(expenseLedger);
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

        // Create expense category
        var expenseCategory = new Category
        {
            Name = "Staff Salary",
            Type = "expense",
            LedgerId = expenseLedger.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Categories.Add(expenseCategory);
        await DbContext.SaveChangesAsync();
        _incomeCategoryId = incomeCategory.Id;
        _expenseCategoryId = expenseCategory.Id;

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

    [Fact]
    public async Task CreateIncomeTransaction_ShouldCreateTransaction_AndUpdateBalances()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new CreateTransactionRequest
        {
            TransactionDate = DateTime.UtcNow,
            Type = "income",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
            PaymentMethod = "cash",
            Description = "Test tuition payment",
            Items = new List<CreateTransactionItemRequest>
            {
                new()
                {
                    CategoryId = _incomeCategoryId,
                    Amount = 500,
                    Quantity = 1,
                    UnitPrice = 500,
                    Description = "Monthly tuition"
                }
            }
        };

        // Act
        var result = await _transactionService.CreateTransactionAsync(request, _testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("income", result.Type);
        Assert.Equal(500, result.Amount);
        Assert.NotEmpty(result.TransactionNumber);
        Assert.NotEmpty(result.ReceiptNumber);

        // Verify journal entries were created (2 entries: 1 debit + 1 credit)
        var journalEntries = DbContext.JournalEntries.Where(j => j.TransactionId == result.Id).ToList();
        Assert.Equal(2, journalEntries.Count);
        Assert.Equal(1000, journalEntries.Sum(j => j.Amount)); // 500 debit + 500 credit

        // Verify one debit and one credit entry
        Assert.Single(journalEntries.Where(j => j.EntryType == "debit"));
        Assert.Single(journalEntries.Where(j => j.EntryType == "credit"));
    }

    [Fact]
    public async Task CreateExpenseTransaction_ShouldCreateTransaction_AndUpdateBalances()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new CreateTransactionRequest
        {
            TransactionDate = DateTime.UtcNow,
            Type = "expense",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
            PaymentMethod = "bank_transfer",
            Description = "Test salary payment",
            Items = new List<CreateTransactionItemRequest>
            {
                new()
                {
                    CategoryId = _expenseCategoryId,
                    Amount = 1000,
                    Quantity = 1,
                    UnitPrice = 1000,
                    Description = "Monthly salary"
                }
            }
        };

        // Act
        var result = await _transactionService.CreateTransactionAsync(request, _testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("expense", result.Type);
        Assert.Equal(1000, result.Amount);

        // Verify journal entries were created
        var journalEntries = DbContext.JournalEntries.Where(j => j.TransactionId == result.Id).ToList();
        Assert.Equal(2, journalEntries.Count);

        // Verify one debit and one credit entry
        Assert.Single(journalEntries.Where(j => j.EntryType == "debit"));
        Assert.Single(journalEntries.Where(j => j.EntryType == "credit"));
    }

    [Fact]
    public async Task CreateTransaction_WithMismatchedAmounts_ShouldThrowBusinessRuleException()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new CreateTransactionRequest
        {
            TransactionDate = DateTime.UtcNow,
            Type = "income",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
            Items = new List<CreateTransactionItemRequest>
            {
                new()
                {
                    CategoryId = _incomeCategoryId,
                    Amount = 500, // Amount doesn't match quantity * unitPrice
                    Quantity = 2,
                    UnitPrice = 100
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<Common.Exceptions.BusinessRuleException>(
            () => _transactionService.CreateTransactionAsync(request, _testUserId));
    }

    [Fact]
    public async Task CreateTransaction_ForClosedYear_ShouldThrowBusinessRuleException()
    {
        // Arrange
        await SetupTestDataAsync();

        // Close the year
        var closedYear = new ClosedYear
        {
            Year = 2024,
            ClosedAt = DateTime.UtcNow,
            ClosedBy = _testUserId
        };
        DbContext.ClosedYears.Add(closedYear);
        await DbContext.SaveChangesAsync();

        var request = new CreateTransactionRequest
        {
            TransactionDate = new DateTime(2024, 6, 15), // Date in closed year
            Type = "income",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Common.Exceptions.BusinessRuleException>(
            () => _transactionService.CreateTransactionAsync(request, _testUserId));
        Assert.Contains("closed year", exception.Message);
    }

    [Fact]
    public async Task GetTransaction_ShouldReturnTransactionWithDetails()
    {
        // Arrange
        await SetupTestDataAsync();
        var createRequest = new CreateTransactionRequest
        {
            TransactionDate = DateTime.UtcNow,
            Type = "income",
            TransactableType = "Student",
            StudentId = _studentId,
            CashLedgerId = _cashLedgerId,
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
        var created = await _transactionService.CreateTransactionAsync(createRequest, _testUserId);

        // Act
        var result = await _transactionService.GetTransactionAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Student", result.StudentName);
        Assert.Single(result.Items);
    }
}
