namespace GenoCare.Domain.Inventory;

public abstract record StockMovement
{
    protected StockMovement(Guid id, Guid productId, Guid batchId, BoxQuantity quantity,
        StockCondition condition, string sourceReference, DateTimeOffset occurredAt)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product is required.", nameof(productId));
        if (batchId == Guid.Empty) throw new ArgumentException("Batch is required.", nameof(batchId));
        if (string.IsNullOrWhiteSpace(sourceReference))
            throw new ArgumentException("Source reference is required.", nameof(sourceReference));
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProductId = productId;
        BatchId = batchId;
        Quantity = quantity;
        Condition = condition;
        SourceReference = sourceReference.Trim();
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }
    public Guid ProductId { get; }
    public Guid BatchId { get; }
    public BoxQuantity Quantity { get; }
    public StockCondition Condition { get; }
    public string SourceReference { get; }
    public DateTimeOffset OccurredAt { get; }
    public abstract int SignedQuantity { get; }
}

public sealed record StockReceipt : StockMovement
{
    public StockReceipt(Guid productId, Guid batchId, BoxQuantity quantity, StockCondition condition,
        string sourceReference, DateTimeOffset occurredAt, Guid? id = null)
        : base(id ?? Guid.NewGuid(), productId, batchId, quantity, condition, sourceReference, occurredAt) { }

    public override int SignedQuantity => Quantity.Value;
}

public sealed record OpeningStock : StockMovement
{
    public OpeningStock(Guid productId, Guid batchId, BoxQuantity quantity, StockCondition condition,
        string sourceReference, DateTimeOffset occurredAt, Guid? id = null)
        : base(id ?? Guid.NewGuid(), productId, batchId, quantity, condition, sourceReference, occurredAt) { }

    public override int SignedQuantity => Quantity.Value;
}

public sealed record StockIssue : StockMovement
{
    public StockIssue(Guid productId, Guid batchId, BoxQuantity quantity, StockCondition condition,
        string sourceReference, DateTimeOffset occurredAt, Guid? id = null)
        : base(id ?? Guid.NewGuid(), productId, batchId, quantity, condition, sourceReference, occurredAt) { }

    public override int SignedQuantity => -Quantity.Value;
}

public sealed record StockAdjustment : StockMovement
{
    public StockAdjustment(Guid productId, Guid batchId, BoxQuantity quantity, AdjustmentDirection direction,
        StockCondition condition, string sourceReference, DateTimeOffset occurredAt, Guid? id = null)
        : base(id ?? Guid.NewGuid(), productId, batchId, quantity, condition, sourceReference, occurredAt)
        => Direction = direction;

    public AdjustmentDirection Direction { get; }
    public override int SignedQuantity => Direction == AdjustmentDirection.Increase ? Quantity.Value : -Quantity.Value;
}

public sealed record InventoryStatusTransition(Guid ProductId, Guid BatchId, StockCondition From,
    StockCondition To, BoxQuantity Quantity, string SourceReference, DateTimeOffset OccurredAt);
