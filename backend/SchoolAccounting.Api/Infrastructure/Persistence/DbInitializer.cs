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
    }
}
