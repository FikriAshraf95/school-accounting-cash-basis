using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace SchoolAccounting.Api.Infrastructure.Persistence;

public class AuditInterceptor : SaveChangesInterceptor
{
    // Only audit these financial/business entities — not every table
    private static readonly HashSet<Type> AuditedTypes =
    [
        typeof(Transaction),
        typeof(Student),
        typeof(Ledger),
        typeof(Payer)
    ];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
            AddAuditLogs(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(AppDbContext context)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
        var timestamp = DateTime.UtcNow;

        var entries = context.ChangeTracker.Entries()
            .Where(e => AuditedTypes.Contains(e.Entity.GetType()) &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            context.AuditLogs.Add(new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                // For Added entities the auto-increment ID is not yet assigned; null is acceptable
                EntityId = entry.State == EntityState.Added
                    ? null
                    : entry.Property("Id").CurrentValue?.ToString(),
                Action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Modified",
                    EntityState.Deleted => "Deleted",
                    _ => entry.State.ToString()
                },
                UserId = userId,
                UserName = userName,
                Timestamp = timestamp
            });
        }
    }
}
