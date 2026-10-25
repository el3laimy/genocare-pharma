using GenoCare.Domain.Catalog;

namespace GenoCare.Application.Catalog;

public sealed class ProductCatalogUseCases(IProductCatalogStore store)
{
    public Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
        => store.SearchAsync(query, cancellationToken);

    public async Task<Product> CreateAsync(ProductDraft draft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var product = Product.Create(draft.InternalCode, draft.ArabicName, draft.RetailPrice,
            draft.PurchasePrice, draft.Description);
        await store.AddAsync(product, cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, ProductDraft draft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var product = await store.GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("الصنف غير موجود.");
        product.Rename(draft.ArabicName, draft.Description);
        product.ChangePrices(draft.RetailPrice, draft.PurchasePrice);
        await store.UpdateAsync(product, cancellationToken);
        return product;
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var product = await store.GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("الصنف غير موجود.");
        product.SetActive(isActive);
        await store.UpdateAsync(product, cancellationToken);
    }
}

public sealed class BatchCatalogUseCases(IBatchCatalogStore store)
{
    public Task<IReadOnlyList<Batch>> ListForProductAsync(Guid productId, CancellationToken cancellationToken = default)
        => store.ListForProductAsync(productId, cancellationToken);

    public async Task<Batch> CreateAsync(Guid productId, string batchNumber, DateOnly expiryDate,
        CancellationToken cancellationToken = default)
    {
        var batch = Batch.Create(productId, batchNumber, expiryDate);
        await store.AddAsync(batch, cancellationToken);
        return batch;
    }
}
