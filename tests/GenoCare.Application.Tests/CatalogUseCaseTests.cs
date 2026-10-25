using GenoCare.Application.Catalog;
using GenoCare.Domain.Catalog;
using Xunit;

namespace GenoCare.Application.Tests;

public sealed class CatalogUseCaseTests
{
    [Fact]
    public async Task ProductUseCaseCreatesAndUpdatesThroughStoreContract()
    {
        var store = new FakeProductStore();
        var useCases = new ProductCatalogUseCases(store);
        var created = await useCases.CreateAsync(new ProductDraft("C-1", "صنف", "وصف", 10, 6));
        await useCases.UpdateAsync(created.Id, new ProductDraft("C-1", "صنف محدث", null, 11, 7));
        Assert.Equal("صنف محدث", store.Product!.ArabicName);
        Assert.Equal(11, store.Product.CurrentRetailPrice);
    }

    [Fact]
    public async Task BatchUseCaseRejectsInvalidExpiryBeforeStoreCall()
    {
        var store = new FakeBatchStore();
        var useCases = new BatchCatalogUseCases(store);
        await Assert.ThrowsAsync<ArgumentException>(() => useCases.CreateAsync(Guid.NewGuid(), "LOT", default));
        Assert.Null(store.Batch);
    }

    private sealed class FakeProductStore : IProductCatalogStore
    {
        public Product? Product { get; private set; }
        public Task AddAsync(Product product, CancellationToken cancellationToken = default) { Product = product; return Task.CompletedTask; }
        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) { Product = product; return Task.CompletedTask; }
        public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Product?.Id == id ? Product : null);
        public Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Product>>(Product is null ? [] : [Product]);
    }

    private sealed class FakeBatchStore : IBatchCatalogStore
    {
        public Batch? Batch { get; private set; }
        public Task AddAsync(Batch batch, CancellationToken cancellationToken = default) { Batch = batch; return Task.CompletedTask; }
        public Task<Batch?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Batch?.Id == id ? Batch : null);
        public Task<IReadOnlyList<Batch>> ListForProductAsync(Guid productId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Batch>>(Batch is null ? [] : [Batch]);
    }
}
