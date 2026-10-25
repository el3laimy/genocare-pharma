using GenoCare.Domain.Inventory;

namespace GenoCare.Infrastructure.Persistence;

public enum PersistedMovementKind
{
    Receipt = 1,
    OpeningStock = 2,
    Issue = 3,
    Adjustment = 4
}

public sealed class StockMovementRecord
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public int EntryIndex { get; set; }
    public Guid ProductId { get; set; }
    public Guid BatchId { get; set; }
    public int Quantity { get; set; }
    public StockCondition Condition { get; set; }
    public AdjustmentDirection? Direction { get; set; }
    public PersistedMovementKind Kind { get; set; }
    public string SourceReference { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}
