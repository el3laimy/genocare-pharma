using GenoCare.Domain.Catalog;
using GenoCare.Application.Catalog;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GenoCare.Infrastructure.Persistence;

public sealed class CatalogPersistence(IDbContextFactory<GenoCareDbContext> contextFactory) : IProductCatalogStore, IBatchCatalogStore
{
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Products.Add(product);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        { throw new InvalidOperationException("الكود الداخلي مستخدم بالفعل أو بيانات الصنف مكررة.", exception); }
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken = default) => AddProductAsync(product, cancellationToken);

    public async Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Products.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default) => GetProductAsync(id, cancellationToken);

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Products.Update(product);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        { throw new InvalidOperationException("الكود الداخلي مستخدم بالفعل أو بيانات الصنف مكررة.", exception); }
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) => UpdateProductAsync(product, cancellationToken);

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

    public Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken cancellationToken = default) => SearchProductsAsync(query, cancellationToken);

    public async Task AddBatchAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Batches.Add(batch);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException)
        { throw new InvalidOperationException("رقم التشغيلة مستخدم لهذا الصنف أو بيانات التشغيلة مكررة.", exception); }
    }

    public Task AddAsync(Batch batch, CancellationToken cancellationToken = default) => AddBatchAsync(batch, cancellationToken);

    public async Task<Batch?> GetBatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Batches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    Task<Batch?> IBatchCatalogStore.GetAsync(Guid id, CancellationToken cancellationToken)
        => GetBatchAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Batch>> ListForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Batches.Where(x => x.ProductId == productId).OrderBy(x => x.ExpiryDate).ToListAsync(cancellationToken);
    }
}
