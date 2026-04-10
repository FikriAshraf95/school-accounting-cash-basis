using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Infrastructure.Persistence;
using System.Threading;

namespace SchoolAccounting.Api.Tests;

// Prevent parallel execution to avoid shared state issues
[CollectionDefinition("SmokeTests", DisableParallelization = true)]
public class SmokeTestCollection { }

[Collection("SmokeTests")]
public abstract class TestBase : IDisposable
{
    protected AppDbContext DbContext { get; }
    private static int _dbCounter = 0;

    protected TestBase()
    {
        // Use a unique database name per test instance with an incrementing counter
        var dbName = $"TestDb_{Interlocked.Increment(ref _dbCounter)}_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(b => b.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        DbContext = new AppDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
