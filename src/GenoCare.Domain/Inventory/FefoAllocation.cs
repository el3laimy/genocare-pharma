using GenoCare.Domain.Catalog;

namespace GenoCare.Domain.Inventory;

public sealed record EligibleBatchStock
{
    public EligibleBatchStock(Batch batch, int availableQuantity)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentOutOfRangeException.ThrowIfNegative(availableQuantity);
        Batch = batch;
        AvailableQuantity = availableQuantity;
    }

    public Batch Batch { get; }
    public int AvailableQuantity { get; }
}

public sealed record BatchAllocation(Guid BatchId, int Quantity);

public sealed record AllocationProposal(int RequestedQuantity, IReadOnlyList<BatchAllocation> Allocations)
{
    public int AllocatedQuantity => Allocations.Sum(a => a.Quantity);
    public bool IsSufficient => AllocatedQuantity == RequestedQuantity;
}

public static class FefoAllocator
{
    public static AllocationProposal Propose(int requestedQuantity, IEnumerable<EligibleBatchStock> batches)
    {
        if (requestedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedQuantity), "Requested quantity must be positive.");
        ArgumentNullException.ThrowIfNull(batches);

        var remaining = requestedQuantity;
        var allocations = new List<BatchAllocation>();
        foreach (var stock in batches
            .Where(x => x.AvailableQuantity > 0)
            .OrderBy(x => x.Batch.ExpiryDate)
            .ThenBy(x => x.Batch.BatchNumber, StringComparer.Ordinal)
            .ThenBy(x => x.Batch.Id))
        {
            if (remaining == 0) break;
            var quantity = Math.Min(remaining, stock.AvailableQuantity);
            allocations.Add(new BatchAllocation(stock.Batch.Id, quantity));
            remaining -= quantity;
        }
        return new AllocationProposal(requestedQuantity, allocations);
    }
}
