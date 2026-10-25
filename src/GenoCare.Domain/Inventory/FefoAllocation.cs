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

public sealed record BatchAllocation
{
    public BatchAllocation(Guid batchId, int quantity)
    {
        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch is required.", nameof(batchId));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0);
        BatchId = batchId;
        Quantity = quantity;
    }

    public Guid BatchId { get; }
    public int Quantity { get; }
}

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

        var candidates = batches.ToList();
        Guid? productId = null;
        var batchIds = new HashSet<Guid>();
        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            productId ??= candidate.Batch.ProductId;
            if (!batchIds.Add(candidate.Batch.Id))
                throw new ArgumentException("FEFO candidates cannot contain duplicate batch identities.", nameof(batches));
            if (candidate.Batch.ProductId != productId)
                throw new ArgumentException("FEFO candidates must belong to one product.", nameof(batches));
        }

        var remaining = requestedQuantity;
        var allocations = new List<BatchAllocation>();
        foreach (var stock in candidates
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
