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
            var adminPassword = configuration["AdminSeedPassword"]
                ?? throw new InvalidOperationException(
                    "AdminSeedPassword is not configured. Set it via the ADMINSEEDPASSWORD environment variable or appsettings.");
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

        // Seed sample reference data (grades, classes, students, payers) for development
        await SeedSampleReferenceDataAsync(dbContext);
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

    private static async Task SeedSampleReferenceDataAsync(AppDbContext dbContext)
    {
        // Only seed sample data if explicitly enabled or in development with no data
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment != "Development")
        {
            return; // Only auto-seed sample data in development
        }

        // Skip if any students already exist
        if (await dbContext.Students.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Seed sample grades
        var grades = new[]
        {
            new StudentGrade { Name = "Primary 1", Code = "P1", Description = "Primary Year 1", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentGrade { Name = "Primary 2", Code = "P2", Description = "Primary Year 2", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentGrade { Name = "Primary 3", Code = "P3", Description = "Primary Year 3", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentGrade { Name = "Secondary 1", Code = "S1", Description = "Secondary Year 1", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentGrade { Name = "Secondary 2", Code = "S2", Description = "Secondary Year 2", IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        dbContext.StudentGrades.AddRange(grades);
        await dbContext.SaveChangesAsync();

        // Seed sample classes
        var classes = new[]
        {
            new StudentClass { Name = "1A", Code = "P1-A", GradeId = grades[0].Id, Section = "A", Capacity = 30, FeeAmount = 500, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "1B", Code = "P1-B", GradeId = grades[0].Id, Section = "B", Capacity = 30, FeeAmount = 500, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "2A", Code = "P2-A", GradeId = grades[1].Id, Section = "A", Capacity = 30, FeeAmount = 550, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "2B", Code = "P2-B", GradeId = grades[1].Id, Section = "B", Capacity = 30, FeeAmount = 550, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "3A", Code = "P3-A", GradeId = grades[2].Id, Section = "A", Capacity = 30, FeeAmount = 600, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "S1A", Code = "S1-A", GradeId = grades[3].Id, Section = "A", Capacity = 35, FeeAmount = 800, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new StudentClass { Name = "S2A", Code = "S2-A", GradeId = grades[4].Id, Section = "A", Capacity = 35, FeeAmount = 850, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        dbContext.StudentClasses.AddRange(classes);
        await dbContext.SaveChangesAsync();

        // Seed sample students
        var students = new[]
        {
            new Student { StudentId = "STU001", Name = "Ahmad bin Abdullah", ClassId = classes[0].Id, GradeId = grades[0].Id, Email = "ahmad.parent@email.com", Phone = "012-3456789", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU002", Name = "Siti binti Ibrahim", ClassId = classes[0].Id, GradeId = grades[0].Id, Email = "siti.parent@email.com", Phone = "013-4567890", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU003", Name = "Muhammad Rizal", ClassId = classes[1].Id, GradeId = grades[0].Id, Email = "rizal.parent@email.com", Phone = "014-5678901", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU004", Name = "Nurul Huda", ClassId = classes[2].Id, GradeId = grades[1].Id, Email = "nurul.parent@email.com", Phone = "015-6789012", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU005", Name = "Lim Wei Ming", ClassId = classes[2].Id, GradeId = grades[1].Id, Email = "lim.parent@email.com", Phone = "016-7890123", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU006", Name = "Tan Mei Ling", ClassId = classes[3].Id, GradeId = grades[1].Id, Email = "tan.parent@email.com", Phone = "017-8901234", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU007", Name = "Rajesh Kumar", ClassId = classes[4].Id, GradeId = grades[2].Id, Email = "rajesh.parent@email.com", Phone = "018-9012345", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU008", Name = "Sarah Johnson", ClassId = classes[4].Id, GradeId = grades[2].Id, Email = "sarah.parent@email.com", Phone = "019-0123456", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU009", Name = "Ali Hassan", ClassId = classes[5].Id, GradeId = grades[3].Id, Email = "ali.parent@email.com", Phone = "011-1234567", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Student { StudentId = "STU010", Name = "Wong Siew Lee", ClassId = classes[6].Id, GradeId = grades[4].Id, Email = "wong.parent@email.com", Phone = "010-2345678", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        dbContext.Students.AddRange(students);
        await dbContext.SaveChangesAsync();

        // Seed sample payers
        var payers = new[]
        {
            new Payer { PayerCode = "DONOR001", Name = "YB Wong's Foundation", Type = "donor", Category = "corporate", Email = "foundation@ybwong.org", Phone = "03-12345678", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "DONOR002", Name = "Lee Family Trust", Type = "donor", Category = "individual", Email = "lee.trust@email.com", Phone = "03-23456789", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "SPONSOR001", Name = "TechCorp Malaysia", Type = "sponsor", Category = "corporate", Email = "csr@techcorp.my", Phone = "03-34567890", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "VENDOR001", Name = "ABC Stationery Supply", Type = "vendor", Category = "corporate", Email = "sales@abcstationery.my", Phone = "03-45678901", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "VENDOR002", Name = "Smart IT Solutions", Type = "vendor", Category = "corporate", Email = "info@smartit.my", Phone = "03-56789012", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "SUPP001", Name = "Global Book Distributors", Type = "supplier", Category = "corporate", Email = "orders@globalbooks.my", Phone = "03-67890123", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Payer { PayerCode = "GOVT001", Name = "Ministry of Education", Type = "government", Category = "government", Email = "grants@moe.gov.my", Phone = "03-78901234", Balance = 0, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        dbContext.Payers.AddRange(payers);
        await dbContext.SaveChangesAsync();
    }
}
