using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Transactions;

public class TransactionService
{
    private readonly AppDbContext _dbContext;

    public TransactionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TransactionListItemResponse>> GetTransactionsAsync(
        int page = 1,
        int perPage = 15,
        string? sortBy = null,
        bool sortDesc = false,
        string? type = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? transactableType = null,
        int? transactableId = null)
    {
        var query = _dbContext.Transactions
            .Where(t => t.DeletedAt == null)
            .Include(t => t.Student)
            .Include(t => t.Payer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type.ToLower());
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= dateTo.Value);
        }

        if (!string.IsNullOrEmpty(transactableType) && transactableId.HasValue)
        {
            if (transactableType.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.StudentId == transactableId.Value);
            }
            else if (transactableType.Equals("Payer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.PayerId == transactableId.Value);
            }
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "transactiondate" => sortDesc ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate),
            "amount" => sortDesc ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
            _ => sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        var total = await query.CountAsync();

        var transactions = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResult<TransactionListItemResponse>
        {
            Data = transactions.Select(t => t.ToListItemResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage > 0 ? lastPage : 1
            }
        };
    }

    public async Task<TransactionResponse> GetTransactionAsync(int id)
    {
        var transaction = await _dbContext.Transactions
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Include(t => t.Student)
            .Include(t => t.Payer)
            .Include(t => t.CashLedger)
            .Include(t => t.CreatedByUser)
            .Include(t => t.UpdatedByUser)
            .Include(t => t.Items)
            .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Transaction with ID {id} not found");

        return transaction.ToResponse();
    }

    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, int createdByUserId)
    {
        // Validate transaction type
        if (request.Type != "income" && request.Type != "expense")
        {
            throw new BusinessRuleException($"Invalid transaction type: {request.Type}. Must be 'income' or 'expense'");
        }

        // Validate transactable type and ID
        if (request.TransactableType != "Student" && request.TransactableType != "Payer")
        {
            throw new BusinessRuleException($"Invalid transactable type: {request.TransactableType}. Must be 'Student' or 'Payer'");
        }

        if (request.TransactableType == "Student" && !request.StudentId.HasValue)
        {
            throw new BusinessRuleException("StudentId is required when TransactableType is 'Student'");
        }

        if (request.TransactableType == "Payer" && !request.PayerId.HasValue)
        {
            throw new BusinessRuleException("PayerId is required when TransactableType is 'Payer'");
        }

        // Validate student/payer exists and is active
        if (request.StudentId.HasValue)
        {
            var student = await _dbContext.Students
                .Where(s => s.Id == request.StudentId.Value && s.DeletedAt == null)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException($"Student with ID {request.StudentId.Value} not found");

            if (!student.IsActive)
            {
                throw new BusinessRuleException("Cannot create transaction for inactive student");
            }
        }

        if (request.PayerId.HasValue)
        {
            var payer = await _dbContext.Payers
                .Where(p => p.Id == request.PayerId.Value && p.DeletedAt == null)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException($"Payer with ID {request.PayerId.Value} not found");

            if (!payer.IsActive)
            {
                throw new BusinessRuleException("Cannot create transaction for inactive payer");
            }
        }

        // Validate cash ledger
        var cashLedger = await _dbContext.Ledgers
            .Where(l => l.Id == request.CashLedgerId && l.DeletedAt == null && l.IsActive)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Cash ledger with ID {request.CashLedgerId} not found");

        if (cashLedger.Type != "asset")
        {
            throw new BusinessRuleException("Cash ledger must be an asset account");
        }

        // Check period lock
        var transactionYear = request.TransactionDate.Year;
        var isYearClosed = await _dbContext.ClosedYears.AnyAsync(cy => cy.Year == transactionYear);
        if (isYearClosed)
        {
            throw new BusinessRuleException($"Cannot create transaction for closed year {transactionYear}");
        }

        // Validate items
        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("Transaction must have at least one item");
        }

        var totalAmount = 0m;
        var transactionItems = new List<TransactionItem>();

        foreach (var item in request.Items)
        {
            var category = await _dbContext.Categories
                .Include(c => c.Ledger)
                .Where(c => c.Id == item.CategoryId && c.IsActive)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException($"Category with ID {item.CategoryId} not found");

            // Validate category type matches transaction type
            if (category.Type != request.Type)
            {
                throw new BusinessRuleException($"Category '{category.Name}' is not valid for {request.Type} transactions");
            }

            // Check if category requires student
            if (category.RequiresStudent && request.TransactableType != "Student")
            {
                throw new BusinessRuleException($"Category '{category.Name}' requires a student");
            }

            var calculatedAmount = item.Quantity * item.UnitPrice;
            if (calculatedAmount != item.Amount)
            {
                throw new BusinessRuleException($"Item amount {item.Amount} does not match calculated amount {calculatedAmount}");
            }

            totalAmount += item.Amount;

            transactionItems.Add(new TransactionItem
            {
                CategoryId = item.CategoryId,
                Amount = item.Amount,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Generate transaction number
        var transactionNumber = GenerateTransactionNumber(request.TransactionDate);

        // Create transaction
        var transaction = new Transaction
        {
            TransactionNumber = transactionNumber,
            TransactionDate = request.TransactionDate,
            Type = request.Type.ToLower(),
            TransactableType = request.TransactableType,
            StudentId = request.StudentId,
            PayerId = request.PayerId,
            Amount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Description = request.Description,
            ReceiptNumber = request.Type == "income" ? GenerateReceiptNumber() : null,
            CashLedgerId = request.CashLedgerId,
            IsReversed = false,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = transactionItems
        };

        // Create journal entries (double-entry)
        var journalEntries = CreateJournalEntries(transaction, transactionItems, createdByUserId);

        // Validate debit = credit
        var totalDebits = journalEntries.Where(j => j.EntryType == "debit").Sum(j => j.Amount);
        var totalCredits = journalEntries.Where(j => j.EntryType == "credit").Sum(j => j.Amount);

        if (totalDebits != totalCredits)
        {
            throw new BusinessRuleException($"Journal entries are unbalanced: debits={totalDebits}, credits={totalCredits}");
        }

        transaction.JournalEntries = journalEntries;

        // Use database transaction for atomicity
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Update ledger balances
            foreach (var entry in journalEntries)
            {
                await UpdateLedgerBalanceAsync(entry.LedgerId, entry.EntryType, entry.Amount);
            }

            // Update student/payer balance
            if (request.StudentId.HasValue)
            {
                await UpdateStudentBalanceAsync(request.StudentId.Value, request.Type, totalAmount);
            }

            if (request.PayerId.HasValue)
            {
                await UpdatePayerBalanceAsync(request.PayerId.Value, request.Type, totalAmount);
            }

            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        // Reload with includes
        await _dbContext.Entry(transaction)
            .Reference(t => t.Student)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Reference(t => t.Payer)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Reference(t => t.CashLedger)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Collection(t => t.Items)
            .LoadAsync();

        foreach (var item in transaction.Items)
        {
            await _dbContext.Entry(item)
                .Reference(i => i.Category)
                .LoadAsync();
        }

        return transaction.ToResponse();
    }

    public async Task<TransactionResponse> UpdateTransactionAsync(int id, UpdateTransactionRequest request, int updatedByUserId)
    {
        var transaction = await _dbContext.Transactions
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Include(t => t.Items)
            .Include(t => t.JournalEntries)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Transaction with ID {id} not found");

        if (transaction.IsReversed)
        {
            throw new BusinessRuleException("Cannot edit a reversed transaction");
        }

        // Check same-day edit restriction
        var today = DateTime.UtcNow.Date;
        var transactionDate = transaction.TransactionDate.Date;
        if (transactionDate != today)
        {
            throw new BusinessRuleException("Transactions can only be edited on the same day they were created");
        }

        // Check period lock
        var transactionYear = request.TransactionDate.Year;
        var isYearClosed = await _dbContext.ClosedYears.AnyAsync(cy => cy.Year == transactionYear);
        if (isYearClosed)
        {
            throw new BusinessRuleException($"Cannot edit transaction for closed year {transactionYear}");
        }

        // Reverse old journal entries
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Reverse ledger balances
            foreach (var entry in transaction.JournalEntries)
            {
                await UpdateLedgerBalanceAsync(entry.LedgerId, entry.EntryType == "debit" ? "credit" : "debit", entry.Amount);
            }

            // Reverse student/payer balance
            if (transaction.StudentId.HasValue)
            {
                await UpdateStudentBalanceAsync(transaction.StudentId.Value, transaction.Type == "income" ? "expense" : "income", transaction.Amount);
            }

            if (transaction.PayerId.HasValue)
            {
                await UpdatePayerBalanceAsync(transaction.PayerId.Value, transaction.Type == "income" ? "expense" : "income", transaction.Amount);
            }

            // Remove old items and journal entries
            _dbContext.TransactionItems.RemoveRange(transaction.Items);
            _dbContext.JournalEntries.RemoveRange(transaction.JournalEntries);

            // Validate new items
            if (request.Items.Count == 0)
            {
                throw new BusinessRuleException("Transaction must have at least one item");
            }

            var totalAmount = 0m;
            var newItems = new List<TransactionItem>();

            foreach (var item in request.Items)
            {
                var category = await _dbContext.Categories
                    .Include(c => c.Ledger)
                    .Where(c => c.Id == item.CategoryId && c.IsActive)
                    .FirstOrDefaultAsync()
                    ?? throw new NotFoundException($"Category with ID {item.CategoryId} not found");

                // Validate category type matches transaction type
                if (category.Type != transaction.Type)
                {
                    throw new BusinessRuleException($"Category '{category.Name}' is not valid for {transaction.Type} transactions");
                }

                var calculatedAmount = item.Quantity * item.UnitPrice;
                if (calculatedAmount != item.Amount)
                {
                    throw new BusinessRuleException($"Item amount {item.Amount} does not match calculated amount {calculatedAmount}");
                }

                totalAmount += item.Amount;

                newItems.Add(new TransactionItem
                {
                    CategoryId = item.CategoryId,
                    Amount = item.Amount,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Update transaction
            transaction.TransactionDate = request.TransactionDate;
            transaction.Amount = totalAmount;
            transaction.PaymentMethod = request.PaymentMethod;
            transaction.ReferenceNumber = request.ReferenceNumber;
            transaction.Description = request.Description;
            transaction.UpdatedBy = updatedByUserId;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.Items = newItems;

            // Create new journal entries
            var journalEntries = CreateJournalEntries(transaction, newItems, transaction.CreatedBy, updatedByUserId);

            // Validate debit = credit
            var totalDebits = journalEntries.Where(j => j.EntryType == "debit").Sum(j => j.Amount);
            var totalCredits = journalEntries.Where(j => j.EntryType == "credit").Sum(j => j.Amount);

            if (totalDebits != totalCredits)
            {
                throw new BusinessRuleException($"Journal entries are unbalanced: debits={totalDebits}, credits={totalCredits}");
            }

            transaction.JournalEntries = journalEntries;

            await _dbContext.SaveChangesAsync();

            // Update ledger balances
            foreach (var entry in journalEntries)
            {
                await UpdateLedgerBalanceAsync(entry.LedgerId, entry.EntryType, entry.Amount);
            }

            // Update student/payer balance
            if (transaction.StudentId.HasValue)
            {
                await UpdateStudentBalanceAsync(transaction.StudentId.Value, transaction.Type, totalAmount);
            }

            if (transaction.PayerId.HasValue)
            {
                await UpdatePayerBalanceAsync(transaction.PayerId.Value, transaction.Type, totalAmount);
            }

            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        // Reload with includes
        await _dbContext.Entry(transaction)
            .Reference(t => t.Student)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Reference(t => t.Payer)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Reference(t => t.CashLedger)
            .LoadAsync();
        await _dbContext.Entry(transaction)
            .Collection(t => t.Items)
            .LoadAsync();

        foreach (var item in transaction.Items)
        {
            await _dbContext.Entry(item)
                .Reference(i => i.Category)
                .LoadAsync();
        }

        return transaction.ToResponse();
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _dbContext.Transactions
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Include(t => t.JournalEntries)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Transaction with ID {id} not found");

        if (transaction.IsReversed)
        {
            throw new BusinessRuleException("Cannot delete a reversed transaction");
        }

        // Check same-day delete restriction
        var today = DateTime.UtcNow.Date;
        var transactionDate = transaction.TransactionDate.Date;
        if (transactionDate != today)
        {
            throw new BusinessRuleException("Transactions can only be deleted on the same day they were created");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Reverse ledger balances
            foreach (var entry in transaction.JournalEntries)
            {
                await UpdateLedgerBalanceAsync(entry.LedgerId, entry.EntryType == "debit" ? "credit" : "debit", entry.Amount);
            }

            // Reverse student/payer balance
            if (transaction.StudentId.HasValue)
            {
                await UpdateStudentBalanceAsync(transaction.StudentId.Value, transaction.Type == "income" ? "expense" : "income", transaction.Amount);
            }

            if (transaction.PayerId.HasValue)
            {
                await UpdatePayerBalanceAsync(transaction.PayerId.Value, transaction.Type == "income" ? "expense" : "income", transaction.Amount);
            }

            transaction.DeletedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private List<JournalEntry> CreateJournalEntries(Transaction transaction, List<TransactionItem> items, int createdBy, int? updatedBy = null)
    {
        var entries = new List<JournalEntry>();
        var entryDate = transaction.TransactionDate;
        var actualCreatedBy = updatedBy ?? createdBy;

        if (transaction.Type == "income")
        {
            // Income: Debit CashLedger for total, Credit each category's ledger
            entries.Add(new JournalEntry
            {
                LedgerId = transaction.CashLedgerId,
                EntryType = "debit",
                Amount = transaction.Amount,
                EntryDate = entryDate,
                Description = $"Cash received: {transaction.Description}",
                JournalType = "transaction",
                CreatedBy = actualCreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            foreach (var item in items)
            {
                var category = _dbContext.Categories
                    .Include(c => c.Ledger)
                    .First(c => c.Id == item.CategoryId);

                entries.Add(new JournalEntry
                {
                    LedgerId = category.LedgerId,
                    EntryType = "credit",
                    Amount = item.Amount,
                    EntryDate = entryDate,
                    Description = item.Description ?? $"Income: {category.Name}",
                    JournalType = "transaction",
                    CreatedBy = actualCreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        else // expense
        {
            // Expense: Debit each category's ledger, Credit CashLedger for total
            foreach (var item in items)
            {
                var category = _dbContext.Categories
                    .Include(c => c.Ledger)
                    .First(c => c.Id == item.CategoryId);

                entries.Add(new JournalEntry
                {
                    LedgerId = category.LedgerId,
                    EntryType = "debit",
                    Amount = item.Amount,
                    EntryDate = entryDate,
                    Description = item.Description ?? $"Expense: {category.Name}",
                    JournalType = "transaction",
                    CreatedBy = actualCreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            entries.Add(new JournalEntry
            {
                LedgerId = transaction.CashLedgerId,
                EntryType = "credit",
                Amount = transaction.Amount,
                EntryDate = entryDate,
                Description = $"Cash paid: {transaction.Description}",
                JournalType = "transaction",
                CreatedBy = actualCreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return entries;
    }

    private async Task UpdateLedgerBalanceAsync(int ledgerId, string entryType, decimal amount)
    {
        var ledger = await _dbContext.Ledgers.FindAsync(ledgerId)
            ?? throw new NotFoundException($"Ledger with ID {ledgerId} not found");

        // Double-entry bookkeeping balance rule:
        //
        //   Account type   | Normal side | Debit effect | Credit effect
        //   --------------------------------------------------------
        //   asset          | debit       | +amount      | -amount
        //   expense        | debit       | +amount      | -amount
        //   liability      | credit      | -amount      | +amount
        //   equity         | credit      | -amount      | +amount
        //   revenue        | credit      | -amount      | +amount
        //
        // Example — income transaction (student pays MYR 500 tuition fee):
        //   DR Cash (asset)          +500   → balance increases
        //   CR Tuition Fee (revenue) +500   → balance increases
        //
        // Example — expense transaction (MYR 1000 salary paid):
        //   DR Salaries (expense)    +1000  → balance increases
        //   CR Cash (asset)          -1000  → balance decreases
        var isDebitNormal = ledger.Type == "asset" || ledger.Type == "expense";

        if (entryType == TransactionConstants.EntryDebit)
        {
            ledger.Balance += isDebitNormal ? amount : -amount;
        }
        else // credit
        {
            ledger.Balance += isDebitNormal ? -amount : amount;
        }

        ledger.UpdatedAt = DateTime.UtcNow;
    }

    private async Task UpdateStudentBalanceAsync(int studentId, string transactionType, decimal amount)
    {
        var student = await _dbContext.Students.FindAsync(studentId)
            ?? throw new NotFoundException($"Student with ID {studentId} not found");

        // Income adds to balance (positive = net receipts), expense subtracts
        if (transactionType == "income")
        {
            student.Balance += amount;
        }
        else
        {
            student.Balance -= amount;
        }

        student.UpdatedAt = DateTime.UtcNow;
    }

    private async Task UpdatePayerBalanceAsync(int payerId, string transactionType, decimal amount)
    {
        var payer = await _dbContext.Payers.FindAsync(payerId)
            ?? throw new NotFoundException($"Payer with ID {payerId} not found");

        // Income adds to balance (positive = net receipts), expense subtracts
        if (transactionType == "income")
        {
            payer.Balance += amount;
        }
        else
        {
            payer.Balance -= amount;
        }

        payer.UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateTransactionNumber(DateTime transactionDate)
    {
        // Uses a random 8-char hex suffix instead of a sequential counter to eliminate
        // the read-then-write race condition. The unique index on TransactionNumber
        // still catches the astronomically unlikely collision at the database level.
        var datePrefix = transactionDate.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{TransactionConstants.TxnPrefix}{datePrefix}-{suffix}";
    }

    private static string GenerateReceiptNumber()
    {
        return $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}

public static class TransactionMappings
{
    public static TransactionResponse ToResponse(this Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            TransactableType = transaction.TransactableType,
            StudentId = transaction.StudentId,
            StudentName = transaction.Student?.Name,
            PayerId = transaction.PayerId,
            PayerName = transaction.Payer?.Name,
            Amount = transaction.Amount,
            PaymentMethod = transaction.PaymentMethod,
            ReferenceNumber = transaction.ReferenceNumber,
            Description = transaction.Description,
            ReceiptNumber = transaction.ReceiptNumber,
            CashLedgerId = transaction.CashLedgerId,
            CashLedgerName = transaction.CashLedger?.Name,
            IsReversed = transaction.IsReversed,
            ReversalTransactionId = transaction.ReversalTransactionId,
            CreatedBy = transaction.CreatedBy,
            CreatedByName = transaction.CreatedByUser?.Name,
            UpdatedBy = transaction.UpdatedBy,
            UpdatedByName = transaction.UpdatedByUser?.Name,
            DeletedAt = transaction.DeletedAt,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            Items = transaction.Items?.Select(i => i.ToResponse()).ToList() ?? []
        };
    }

    public static TransactionListItemResponse ToListItemResponse(this Transaction transaction)
    {
        string? transactableName = null;
        if (transaction.TransactableType == "Student" && transaction.Student != null)
        {
            transactableName = transaction.Student.Name;
        }
        else if (transaction.TransactableType == "Payer" && transaction.Payer != null)
        {
            transactableName = transaction.Payer.Name;
        }

        return new TransactionListItemResponse
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            TransactableName = transactableName,
            Amount = transaction.Amount,
            PaymentMethod = transaction.PaymentMethod,
            Description = transaction.Description,
            IsReversed = transaction.IsReversed,
            CreatedAt = transaction.CreatedAt
        };
    }

    public static TransactionItemResponse ToResponse(this TransactionItem item)
    {
        return new TransactionItemResponse
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name,
            Amount = item.Amount,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };
    }
}
