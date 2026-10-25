using GenoCare.Domain.Catalog;
using GenoCare.Domain.Inventory;

namespace GenoCare.Application.Catalog;

public sealed record ProductDraft(string InternalCode, string ArabicName, string? Description,
    decimal RetailPrice, decimal PurchasePrice);

public interface IProductCatalogStore
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public interface IBatchCatalogStore
{
    Task AddAsync(Batch batch, CancellationToken cancellationToken = default);
    Task<Batch?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Batch>> ListForProductAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IInventoryReadStore
{
    Task<StockBalance> GetBalanceAsync(Guid batchId, CancellationToken cancellationToken = default);
}
