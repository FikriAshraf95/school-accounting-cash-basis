using Microsoft.EntityFrameworkCore;

namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; } = null!;
    public DbSet<StudentGrade> StudentGrades { get; set; } = null!;
    public DbSet<StudentClass> StudentClasses { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Ledger> Ledgers { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<BusinessInfo> BusinessInfos { get; set; } = null!;
    public DbSet<Payer> Payers { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<TransactionItem> TransactionItems { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<ClosedYear> ClosedYears { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // PersonalAccessTokens
        modelBuilder.Entity<PersonalAccessToken>(entity =>
        {
            entity.ToTable("PersonalAccessTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Token).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token).IsUnique();
        });

        // StudentGrades
        modelBuilder.Entity<StudentGrade>(entity =>
        {
            entity.ToTable("StudentGrades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // StudentClasses
        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.ToTable("StudentClasses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Section).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FeeAmount).HasPrecision(15, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.Grade)
                .WithMany(g => g.Classes)
                .HasForeignKey(e => e.GradeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Students
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.StudentId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Balance).HasPrecision(15, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.StudentId).IsUnique();

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Grade)
                .WithMany()
                .HasForeignKey(e => e.GradeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Ledgers
        modelBuilder.Entity<Ledger>(entity =>
        {
            entity.ToTable("Ledgers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Balance).HasPrecision(15, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Categories
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Ledger)
                .WithMany(l => l.Categories)
                .HasForeignKey(e => e.LedgerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BusinessInfos
        modelBuilder.Entity<BusinessInfo>(entity =>
        {
            entity.ToTable("BusinessInfos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.SchoolName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FinancialYearStart).IsRequired();
            entity.Property(e => e.FinancialYearEnd).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("MYR");
            entity.Property(e => e.Timezone).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.BankAccountName).HasMaxLength(255);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(100);
            entity.Property(e => e.TaxRegistration).HasMaxLength(100);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Payers
        modelBuilder.Entity<Payer>(entity =>
        {
            entity.ToTable("Payers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.PayerCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Balance).HasPrecision(15, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.PayerCode).IsUnique();
        });

        // ClosedYears
        modelBuilder.Entity<ClosedYear>(entity =>
        {
            entity.ToTable("ClosedYears");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.ClosedAt).IsRequired();

            entity.HasIndex(e => e.Year).IsUnique();

            entity.HasOne(e => e.ClosedByUser)
                .WithMany()
                .HasForeignKey(e => e.ClosedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transactions
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TransactableType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(100);
            entity.Property(e => e.IsReversed).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => new { e.TransactionDate, e.Type });
            entity.HasIndex(e => new { e.TransactableType, e.StudentId, e.PayerId });
            entity.HasIndex(e => e.CashLedgerId);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Payer)
                .WithMany()
                .HasForeignKey(e => e.PayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashLedger)
                .WithMany()
                .HasForeignKey(e => e.CashLedgerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReversalTransaction)
                .WithMany()
                .HasForeignKey(e => e.ReversalTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TransactionItems
        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.ToTable("TransactionItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UnitPrice).HasPrecision(15, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.Items)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntries
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.EntryType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.JournalType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.LedgerId, e.EntryDate });
            entity.HasIndex(e => e.TransactionId);

            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.JournalEntries)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Ledger)
                .WithMany()
                .HasForeignKey(e => e.LedgerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
