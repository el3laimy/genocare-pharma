using GenoCare.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GenoCare.Infrastructure.Persistence;

public sealed class CatalogPersistence(IDbContextFactory<GenoCareDbContext> contextFactory)
{
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Products.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Products.Update(product);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var normalized = query.Trim();
        return await db.Products
            .Where(x => x.InternalCode.Contains(normalized) || x.ArabicName.Contains(normalized))
            .OrderBy(x => x.InternalCode)
            .ToListAsync(cancellationToken);
    }

    public async Task AddBatchAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Batches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Batch?> GetBatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Batches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
