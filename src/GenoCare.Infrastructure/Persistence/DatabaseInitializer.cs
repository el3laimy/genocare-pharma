using Microsoft.EntityFrameworkCore;

namespace GenoCare.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbContextFactory<GenoCareDbContext> contextFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
