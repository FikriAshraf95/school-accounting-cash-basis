using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.Transactions;

public class TransactionReversalService
{
    private readonly AppDbContext _dbContext;

    public TransactionReversalService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionResponse> ReverseTransactionAsync(int transactionId, int createdByUserId)
    {
        var originalTransaction = await _dbContext.Transactions
            .Where(t => t.Id == transactionId && t.DeletedAt == null)
            .Include(t => t.Items)
            .Include(t => t.JournalEntries)
            .Include(t => t.Student)
            .Include(t => t.Payer)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Transaction with ID {transactionId} not found");

        if (originalTransaction.IsReversed)
        {
            throw new BusinessRuleException("Transaction has already been reversed");
        }

        // Check period lock for today's year (reversal is always dated today)
        var today = DateTime.UtcNow;
        var currentYear = today.Year;
        var isYearClosed = await _dbContext.ClosedYears.AnyAsync(cy => cy.Year == currentYear);
        if (isYearClosed)
        {
            throw new BusinessRuleException($"Cannot create reversal for closed year {currentYear}");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Create reversal transaction
            var reversalType = originalTransaction.Type == "income" ? "expense" : "income";
            var reversalTransaction = new Transaction
            {
                TransactionNumber = await GenerateReversalTransactionNumberAsync(today),
                TransactionDate = today,
                Type = reversalType,
                TransactableType = originalTransaction.TransactableType,
                StudentId = originalTransaction.StudentId,
                PayerId = originalTransaction.PayerId,
                Amount = originalTransaction.Amount,
                PaymentMethod = originalTransaction.PaymentMethod,
                ReferenceNumber = $"REVERSAL OF {originalTransaction.TransactionNumber}",
                Description = $"Reversal of transaction {originalTransaction.TransactionNumber}: {originalTransaction.Description}",
                ReceiptNumber = null,
                CashLedgerId = originalTransaction.CashLedgerId,
                IsReversed = false,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create reversal items
            var reversalItems = originalTransaction.Items.Select(item => new TransactionItem
            {
                CategoryId = item.CategoryId,
                Amount = item.Amount,
                Description = $"Reversal: {item.Description}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            reversalTransaction.Items = reversalItems;

            // Create reversal journal entries (opposite of original)
            var reversalEntries = new List<JournalEntry>();
            foreach (var originalEntry in originalTransaction.JournalEntries)
            {
                reversalEntries.Add(new JournalEntry
                {
                    LedgerId = originalEntry.LedgerId,
                    EntryType = originalEntry.EntryType == "debit" ? "credit" : "debit",
                    Amount = originalEntry.Amount,
                    EntryDate = today,
                    Description = $"Reversal: {originalEntry.Description}",
                    JournalType = "reversal",
                    CreatedBy = createdByUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            reversalTransaction.JournalEntries = reversalEntries;

            _dbContext.Transactions.Add(reversalTransaction);
            await _dbContext.SaveChangesAsync();

            // Update original transaction
            originalTransaction.IsReversed = true;
            originalTransaction.ReversalTransactionId = reversalTransaction.Id;
            originalTransaction.UpdatedAt = DateTime.UtcNow;

            // Update ledger balances for reversal
            foreach (var entry in reversalEntries)
            {
                await UpdateLedgerBalanceAsync(entry.LedgerId, entry.EntryType, entry.Amount);
            }

            // Update student/payer balance for reversal (opposite of original)
            if (reversalTransaction.StudentId.HasValue)
            {
                await UpdateStudentBalanceAsync(reversalTransaction.StudentId.Value, reversalType, reversalTransaction.Amount);
            }

            if (reversalTransaction.PayerId.HasValue)
            {
                await UpdatePayerBalanceAsync(reversalTransaction.PayerId.Value, reversalType, reversalTransaction.Amount);
            }

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            // Reload with includes
            await _dbContext.Entry(reversalTransaction)
                .Reference(t => t.Student)
                .LoadAsync();
            await _dbContext.Entry(reversalTransaction)
                .Reference(t => t.Payer)
                .LoadAsync();
            await _dbContext.Entry(reversalTransaction)
                .Reference(t => t.CashLedger)
                .LoadAsync();
            await _dbContext.Entry(reversalTransaction)
                .Collection(t => t.Items)
                .LoadAsync();

            foreach (var item in reversalTransaction.Items)
            {
                await _dbContext.Entry(item)
                    .Reference(i => i.Category)
                    .LoadAsync();
            }

            return reversalTransaction.ToResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task UpdateLedgerBalanceAsync(int ledgerId, string entryType, decimal amount)
    {
        var ledger = await _dbContext.Ledgers.FindAsync(ledgerId)
            ?? throw new NotFoundException($"Ledger with ID {ledgerId} not found");

        var isDebitNormal = ledger.Type == "asset" || ledger.Type == "expense";

        if (entryType == "debit")
        {
            ledger.Balance += isDebitNormal ? amount : -amount;
        }
        else
        {
            ledger.Balance += isDebitNormal ? -amount : amount;
        }

        ledger.UpdatedAt = DateTime.UtcNow;
    }

    private async Task UpdateStudentBalanceAsync(int studentId, string transactionType, decimal amount)
    {
        var student = await _dbContext.Students.FindAsync(studentId)
            ?? throw new NotFoundException($"Student with ID {studentId} not found");

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

    private async Task<string> GenerateReversalTransactionNumberAsync(DateTime date)
    {
        var datePrefix = date.ToString("yyyyMMdd");
        var baseNumber = $"REV-{datePrefix}-";

        var lastTransaction = await _dbContext.Transactions
            .Where(t => t.TransactionNumber.StartsWith(baseNumber))
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTransaction != null)
        {
            var lastSequence = lastTransaction.TransactionNumber.Split('-').Last();
            if (int.TryParse(lastSequence, out var seq))
            {
                sequence = seq + 1;
            }
        }

        return $"{baseNumber}{sequence:D4}";
    }
}
