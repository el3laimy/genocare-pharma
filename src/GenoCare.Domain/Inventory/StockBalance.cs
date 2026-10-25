namespace GenoCare.Domain.Inventory;

public readonly record struct StockBalance(int Available, int Quarantined, int Damaged)
{
    public int SaleableQuantity => Available;

    public static StockBalance FromMovements(IEnumerable<StockMovement> movements, Guid batchId)
    {
        ArgumentNullException.ThrowIfNull(movements);
        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch is required.", nameof(batchId));
        var totals = new int[3];
        foreach (var movement in movements)
        {
            ArgumentNullException.ThrowIfNull(movement);
            if (movement.BatchId != batchId) continue;
            InventoryEnumValidation.EnsureCondition(movement.Condition, nameof(movement.Condition));
            var index = (int)movement.Condition - 1;
            totals[index] = checked(totals[index] + movement.SignedQuantity);
        }
        return new StockBalance(totals[0], totals[1], totals[2]);
    }
}
