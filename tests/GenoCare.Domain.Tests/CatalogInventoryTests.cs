using GenoCare.Domain.Catalog;
using GenoCare.Domain.Inventory;
using Xunit;

namespace GenoCare.Domain.Tests;

public sealed class CatalogInventoryTests
{
    private static readonly DateTimeOffset At = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ProductRequiresCodeNameAndNonNegativePrices()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(" ", "دواء", 1, 1));
        Assert.Throws<ArgumentException>(() => Product.Create("A", " ", 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create("A", "دواء", -1, 1));
        Assert.Equal(SellingUnit.Box, Product.Create(" A ", " دواء ", 10, 5).SellingUnit);
    }

    [Fact]
    public void ProductPricesCanChangeWithoutChangingIdentityOrUnit()
    {
        var product = Product.Create("A", "دواء", 10, 5);
        product.ChangePrices(12, 6);
        Assert.Equal(12, product.CurrentRetailPrice);
        Assert.Equal(SellingUnit.Box, product.SellingUnit);
    }

    [Fact]
    public void BatchRequiresProductNumberAndExpiry()
    {
        var productId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => Batch.Create(Guid.Empty, "B", new DateOnly(2027, 1, 1)));
        Assert.Throws<ArgumentException>(() => Batch.Create(productId, " ", new DateOnly(2027, 1, 1)));
        Assert.Throws<ArgumentException>(() => Batch.Create(productId, "B", default));
    }

    [Fact]
    public void MovementRequiresPositiveWholeBoxesAndSourceReference()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BoxQuantity.Positive(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => BoxQuantity.Positive(-1));
        Assert.Throws<ArgumentException>(() => new StockReceipt(Guid.NewGuid(), Guid.NewGuid(),
            BoxQuantity.Positive(1), StockCondition.Available, " ", At));
    }

    [Fact]
    public void BalanceDerivationExcludesQuarantinedAndDamagedFromSaleableStock()
    {
        var productId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var movements = new StockMovement[]
        {
            new StockReceipt(productId, batchId, BoxQuantity.Positive(10), StockCondition.Available, "R1", At),
            new StockReceipt(productId, batchId, BoxQuantity.Positive(4), StockCondition.Quarantined, "R2", At),
            new StockReceipt(productId, batchId, BoxQuantity.Positive(2), StockCondition.Damaged, "R3", At),
            new StockIssue(productId, batchId, BoxQuantity.Positive(3), StockCondition.Available, "S1", At)
        };
        var balance = StockBalance.FromMovements(movements, batchId);
        Assert.Equal(7, balance.Available);
        Assert.Equal(4, balance.Quarantined);
        Assert.Equal(2, balance.Damaged);
        Assert.Equal(7, balance.SaleableQuantity);
    }

    [Fact]
    public void FefoAllocatesNearestExpiryAcrossMultipleBatches()
    {
        var productId = Guid.NewGuid();
        var first = Batch.Create(productId, "B-1", new DateOnly(2026, 3, 1));
        var second = Batch.Create(productId, "B-2", new DateOnly(2026, 2, 1));
        var result = FefoAllocator.Propose(8, new[]
        {
            new EligibleBatchStock(first, 5), new EligibleBatchStock(second, 5)
        });
        Assert.True(result.IsSufficient);
        Assert.Equal(new BatchAllocation(second.Id, 5), result.Allocations[0]);
        Assert.Equal(new BatchAllocation(first.Id, 3), result.Allocations[1]);
    }

    [Fact]
    public void FefoReportsInsufficientStockWithoutOverAllocating()
    {
        var batch = Batch.Create(Guid.NewGuid(), "B", new DateOnly(2027, 1, 1));
        var result = FefoAllocator.Propose(5, new[] { new EligibleBatchStock(batch, 2) });
        Assert.False(result.IsSufficient);
        Assert.Equal(2, result.AllocatedQuantity);
        Assert.Equal(2, result.Allocations.Single().Quantity);
    }

    [Fact]
    public void FefoIsDeterministicWhenExpiryMatches()
    {
        var productId = Guid.NewGuid();
        var a = Batch.Create(productId, "A", new DateOnly(2027, 1, 1), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var b = Batch.Create(productId, "B", new DateOnly(2027, 1, 1), Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var result = FefoAllocator.Propose(1, new[] { new EligibleBatchStock(b, 1), new EligibleBatchStock(a, 1) });
        Assert.Equal(a.Id, result.Allocations[0].BatchId);
    }

    [Fact]
    public void FefoRejectsZeroRequestAndNegativeAvailableQuantity()
    {
        var batch = Batch.Create(Guid.NewGuid(), "B", new DateOnly(2027, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FefoAllocator.Propose(0, Array.Empty<EligibleBatchStock>()));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EligibleBatchStock(batch, -1));
    }
}
