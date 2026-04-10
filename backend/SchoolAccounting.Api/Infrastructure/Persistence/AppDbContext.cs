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
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("PHP");
            entity.Property(e => e.Timezone).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.BankAccountName).HasMaxLength(255);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(100);
            entity.Property(e => e.TaxRegistration).HasMaxLength(100);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}
