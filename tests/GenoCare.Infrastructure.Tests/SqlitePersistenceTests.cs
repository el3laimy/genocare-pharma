using GenoCare.Domain.Catalog;
using GenoCare.Domain.Inventory;
using GenoCare.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace GenoCare.Infrastructure.Tests;

public sealed class SqlitePersistenceTests
{
    private static readonly DateTimeOffset At = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MigrationReopensAndPersistsProductAndBatch()
    {
        await using var database = await TestDatabase.CreateAsync();
        var product = Product.Create("GC-001", "دواء تجريبي", 12.345678901234567890m, 7.123456789012345678m);
        await using (var db = database.CreateContext())
        {
            db.Products.Add(product);
            db.Batches.Add(Batch.Create(product.Id, "LOT-1", new DateOnly(2028, 1, 1)));
            await db.SaveChangesAsync();
        }
        await using (var reopened = database.CreateContext())
        {
            var loaded = await reopened.Products.SingleAsync();
            Assert.Equal(product.CurrentRetailPrice, loaded.CurrentRetailPrice);
            Assert.Equal(product.CurrentPurchasePrice, loaded.CurrentPurchasePrice);
            Assert.Equal("LOT-1", await reopened.Batches.Select(x => x.BatchNumber).SingleAsync());
        }
    }

    [Fact]
    public async Task ProductCodeAndBatchIdentityAreUnique()
    {
        await using var database = await TestDatabase.CreateAsync();
        var product = Product.Create("GC-002", "صنف", 1, 1);
        var batch = Batch.Create(product.Id, "LOT-2", new DateOnly(2028, 1, 1));
        await using var db = database.CreateContext();
        db.Products.Add(product);
        db.Batches.Add(batch);
        await db.SaveChangesAsync();
        db.Products.Add(Product.Create("GC-002", "مكرر", 2, 2));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        db.ChangeTracker.Clear();
        db.Batches.Add(Batch.Create(product.Id, "LOT-2", new DateOnly(2028, 2, 1)));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void DefaultDatabasePathUsesLocalApplicationDataAndCreatesFolder()
    {
        var applicationName = $"GenoCare-Test-{Guid.NewGuid():N}";
        var testRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var path = GenoCareDatabasePaths.GetDefaultPath(applicationName, testRoot);
        var expectedRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Assert.EndsWith(System.IO.Path.Combine(applicationName, "genocare.db"), path, StringComparison.OrdinalIgnoreCase);
        Assert.True(Directory.Exists(System.IO.Path.GetDirectoryName(path)));
        Assert.NotEqual(expectedRoot, testRoot);
        Directory.Delete(testRoot, recursive: true);
    }

    [Fact]
    public async Task CatalogPersistenceCreatesRetrievesSearchesAndAddsBatches()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = new CatalogPersistence(database.Factory);
        var product = Product.Create("GC-SEARCH", "باراسيتامول", 3, 2);
        await catalog.AddProductAsync(product);
        await catalog.AddBatchAsync(Batch.Create(product.Id, "LOT-S", new DateOnly(2029, 1, 1)));
        Assert.Equal(product.Id, (await catalog.GetProductAsync(product.Id))!.Id);
        Assert.Single(await catalog.SearchProductsAsync("باراسيتامول"));
        await using var db = database.CreateContext();
        var batchId = await db.Batches.Select(x => x.Id).SingleAsync();
        Assert.Equal("LOT-S", (await catalog.GetBatchAsync(batchId))!.BatchNumber);
    }

    [Fact]
    public async Task MovementsAreAuthoritativeAndTransitionIsAtomicAndIdempotent()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        var store = new InventoryPersistence(database.Factory);
        await store.PostMovementAsync(new StockReceipt(productId, batchId, BoxQuantity.Positive(10), StockCondition.Available, "R", At));
        var transition = InventoryStatusTransition.Create(productId, batchId, StockCondition.Available,
            StockCondition.Quarantined, BoxQuantity.Positive(3), "T", At, Guid.NewGuid());
        await store.PostTransitionAsync(transition);
        var balance = await store.GetBalanceAsync(batchId);
        Assert.Equal(new StockBalance(7, 3, 0), balance);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.PostTransitionAsync(transition));
        Assert.Equal(balance, await store.GetBalanceAsync(batchId));
        await using var db = database.CreateContext();
        Assert.Equal(3, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task DecreasingAdjustmentCannotMakeAnyConditionNegative()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        var store = new InventoryPersistence(database.Factory);
        foreach (var condition in Enum.GetValues<StockCondition>())
        {
            await store.PostMovementAsync(new StockReceipt(productId, batchId, BoxQuantity.Positive(2), condition, $"R-{condition}", At));
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.PostMovementAsync(
                new StockAdjustment(productId, batchId, BoxQuantity.Positive(3), AdjustmentDirection.Decrease, condition, $"D-{condition}", At)));
        }
        Assert.Equal(new StockBalance(2, 2, 2), await store.GetBalanceAsync(batchId));
    }

    [Fact]
    public async Task ValidAdjustmentDecreaseAndIncreaseArePersisted()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        var store = new InventoryPersistence(database.Factory);
        await store.PostMovementAsync(new StockReceipt(productId, batchId, BoxQuantity.Positive(5), StockCondition.Available, "R", At));
        await store.PostMovementAsync(new StockAdjustment(productId, batchId, BoxQuantity.Positive(2), AdjustmentDirection.Decrease, StockCondition.Available, "D", At));
        await store.PostMovementAsync(new StockAdjustment(productId, batchId, BoxQuantity.Positive(4), AdjustmentDirection.Increase, StockCondition.Damaged, "I", At));
        Assert.Equal(new StockBalance(3, 0, 4), await store.GetBalanceAsync(batchId));
    }

    [Fact]
    public async Task ProductBatchMismatchIsRejectedForMovementAndTransition()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productA = Guid.NewGuid();
        var batchA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var batchB = Guid.NewGuid();
        await database.SeedBatchAsync(productA, batchA);
        await database.SeedBatchAsync(productB, batchB);
        var store = new InventoryPersistence(database.Factory);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.PostMovementAsync(
            new StockReceipt(productA, batchB, BoxQuantity.Positive(1), StockCondition.Available, "BAD", At)));
        var transition = InventoryStatusTransition.Create(productA, batchB, StockCondition.Available,
            StockCondition.Quarantined, BoxQuantity.Positive(1), "BAD-T", At);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.PostTransitionAsync(transition));
        await using var db = database.CreateContext();
        Assert.Empty(await db.StockMovements.ToListAsync());
    }

    [Fact]
    public async Task StockMovementsAreAppendOnlyThroughTheContext()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        var movement = new StockReceipt(productId, batchId, BoxQuantity.Positive(1), StockCondition.Available, "APPEND", At);
        await new InventoryPersistence(database.Factory).PostMovementAsync(movement);
        await using var db = database.CreateContext();
        var record = await db.StockMovements.SingleAsync();
        record.Quantity = 2;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        db.StockMovements.Remove(record);
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task IssueRejectsInsufficientStockAndRollsBack()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        var store = new InventoryPersistence(database.Factory);
        await store.PostMovementAsync(new StockReceipt(productId, batchId, BoxQuantity.Positive(2), StockCondition.Available, "R", At));
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.PostMovementAsync(
            new StockIssue(productId, batchId, BoxQuantity.Positive(3), StockCondition.Available, "I", At)));
        Assert.Equal(2, (await store.GetBalanceAsync(batchId)).Available);
    }

    [Fact]
    public async Task MovementSourceAndDataSurviveContextDisposal()
    {
        await using var database = await TestDatabase.CreateAsync();
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        await database.SeedBatchAsync(productId, batchId);
        await new InventoryPersistence(database.Factory).PostMovementAsync(
            new OpeningStock(productId, batchId, BoxQuantity.Positive(4), StockCondition.Damaged, "OPEN-1", At));
        await using var db = database.CreateContext();
        var record = await db.StockMovements.SingleAsync();
        Assert.Equal("OPEN-1", record.SourceReference);
        Assert.Equal(4, record.Quantity);
        Assert.Equal(StockCondition.Damaged, record.Condition);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(string path, PooledDbContextFactory<GenoCareDbContext> factory)
        {
            Path = path;
            Factory = factory;
        }

        public string Path { get; }
        public PooledDbContextFactory<GenoCareDbContext> Factory { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"genocare-{Guid.NewGuid():N}.db");
            var options = new DbContextOptionsBuilder<GenoCareDbContext>()
                .UseSqlite($"Data Source={path}")
                .Options;
            var factory = new PooledDbContextFactory<GenoCareDbContext>(options);
            await using var db = factory.CreateDbContext();
            await db.Database.MigrateAsync();
            return new TestDatabase(path, factory);
        }

        public GenoCareDbContext CreateContext() => Factory.CreateDbContext();

        public async Task SeedBatchAsync(Guid productId, Guid batchId)
        {
            await using var db = CreateContext();
            db.Products.Add(Product.Create($"TEST-{productId:N}", "اختبار", 1, 1, id: productId));
            db.Batches.Add(Batch.Create(productId, $"LOT-{batchId:N}", new DateOnly(2028, 1, 1), batchId));
            await db.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();
            try { File.Delete(Path); } catch (IOException) { }
            await Task.CompletedTask;
        }
    }
}
