using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext dbContext, IConfiguration configuration)
    {
        await dbContext.Database.MigrateAsync();

        // Seed admin user if no admin exists
        var adminExists = await dbContext.Users.AnyAsync(u => u.Role == AppRole.Admin);
        if (!adminExists)
        {
            var adminPassword = configuration["AdminSeedPassword"] ?? "ChangeMe123!";
            var adminUser = new User
            {
                Name = "Administrator",
                Username = "admin",
                Email = "admin@school.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = AppRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        // Seed default chart of accounts if no ledgers exist
        await SeedDefaultChartOfAccountsAsync(dbContext);

        // Seed default business info if none exists
        await SeedDefaultBusinessInfoAsync(dbContext);
    }

    private static async Task SeedDefaultChartOfAccountsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Ledgers.AnyAsync())
        {
            return; // Already seeded
        }

        var defaultLedgers = new[]
        {
            // Assets (1000-1999)
            new Ledger { Code = "1000", Name = "Cash on Hand", Type = "asset", Category = "Current Assets", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "1010", Name = "Cash in Bank", Type = "asset", Category = "Current Assets", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "1100", Name = "Accounts Receivable", Type = "asset", Category = "Current Assets", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "1200", Name = "Prepaid Expenses", Type = "asset", Category = "Current Assets", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // Liabilities (2000-2999)
            new Ledger { Code = "2000", Name = "Accounts Payable", Type = "liability", Category = "Current Liabilities", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "2100", Name = "Accrued Liabilities", Type = "liability", Category = "Current Liabilities", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // Equity (3000-3999)
            new Ledger { Code = "3000", Name = "Retained Earnings", Type = "equity", Category = "Equity", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "3100", Name = "Owner's Equity", Type = "equity", Category = "Equity", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // Revenue (4000-4999)
            new Ledger { Code = "4000", Name = "Tuition Fee Income", Type = "revenue", Category = "Operating Revenue", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "4010", Name = "Miscellaneous Income", Type = "revenue", Category = "Operating Revenue", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "4020", Name = "Donations Received", Type = "revenue", Category = "Non-Operating Revenue", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "4030", Name = "Government Grants", Type = "revenue", Category = "Non-Operating Revenue", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // Expenses (5000-5999)
            new Ledger { Code = "5000", Name = "Salaries Expense", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5010", Name = "Utilities Expense", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5020", Name = "Office Supplies", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5030", Name = "Maintenance & Repairs", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5040", Name = "School Supplies", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5050", Name = "Transportation Expense", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5060", Name = "Communication Expense", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Ledger { Code = "5070", Name = "Miscellaneous Expense", Type = "expense", Category = "Operating Expenses", Balance = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        dbContext.Ledgers.AddRange(defaultLedgers);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDefaultBusinessInfoAsync(AppDbContext dbContext)
    {
        if (await dbContext.BusinessInfos.AnyAsync())
        {
            return; // Already seeded
        }

        var currentYear = DateTime.UtcNow.Year;
        var businessInfo = new BusinessInfo
        {
            SchoolName = "School Accounting System",
            RegistrationNumber = null,
            Address = null,
            Phone = null,
            Email = null,
            FinancialYearStart = new DateOnly(currentYear, 1, 1),
            FinancialYearEnd = new DateOnly(currentYear, 12, 31),
            Currency = "MYR",
            Timezone = "Asia/Kuala_Lumpur",
            BankName = null,
            BankAccountName = null,
            BankAccountNumber = null,
            TaxRegistration = null,
            TaxRate = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.BusinessInfos.Add(businessInfo);
        await dbContext.SaveChangesAsync();
    }
}
