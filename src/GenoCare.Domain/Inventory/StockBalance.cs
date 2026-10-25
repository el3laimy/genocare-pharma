namespace GenoCare.Domain.Inventory;

public readonly record struct StockBalance(int Available, int Quarantined, int Damaged)
{
    public int SaleableQuantity => Available;

    public static StockBalance FromMovements(IEnumerable<StockMovement> movements, Guid batchId)
    {
        ArgumentNullException.ThrowIfNull(movements);
        var totals = new int[3];
        foreach (var movement in movements)
        {
            if (movement.BatchId != batchId) continue;
            var index = (int)movement.Condition - 1;
            totals[index] += movement.SignedQuantity;
        }
        return new StockBalance(totals[0], totals[1], totals[2]);
    }
}
