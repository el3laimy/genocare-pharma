namespace GenoCare.Domain.Inventory;

public abstract record StockMovement
{
    protected StockMovement(Guid id, Guid productId, Guid batchId, BoxQuantity quantity,
        StockCondition condition, string sourceReference, DateTimeOffset occurredAt)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product is required.", nameof(productId));
        if (batchId == Guid.Empty) throw new ArgumentException("Batch is required.", nameof(batchId));
        quantity.EnsurePositive();
        InventoryEnumValidation.EnsureCondition(condition, nameof(condition));
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
    {
        InventoryEnumValidation.EnsureDirection(direction, nameof(direction));
        Direction = direction;
    }

    public AdjustmentDirection Direction { get; }
    public override int SignedQuantity => Direction == AdjustmentDirection.Increase ? Quantity.Value : -Quantity.Value;
}

public sealed record InventoryStatusTransition
{
    private InventoryStatusTransition(Guid id, Guid productId, Guid batchId, StockCondition from,
        StockCondition to, BoxQuantity quantity, string sourceReference, DateTimeOffset occurredAt)
    {
        if (id == Guid.Empty) throw new ArgumentException("Transition identity cannot be empty.", nameof(id));
        if (productId == Guid.Empty) throw new ArgumentException("Product is required.", nameof(productId));
        if (batchId == Guid.Empty) throw new ArgumentException("Batch is required.", nameof(batchId));
        InventoryEnumValidation.EnsureCondition(from, nameof(from));
        InventoryEnumValidation.EnsureCondition(to, nameof(to));
        if (from == to) throw new ArgumentException("Transition must change stock condition.", nameof(to));
        quantity.EnsurePositive();
        if (string.IsNullOrWhiteSpace(sourceReference))
            throw new ArgumentException("Source reference is required.", nameof(sourceReference));
        Id = id;
        ProductId = productId;
        BatchId = batchId;
        From = from;
        To = to;
        Quantity = quantity;
        SourceReference = sourceReference.Trim();
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }
    public Guid ProductId { get; }
    public Guid BatchId { get; }
    public StockCondition From { get; }
    public StockCondition To { get; }
    public BoxQuantity Quantity { get; }
    public string SourceReference { get; }
    public DateTimeOffset OccurredAt { get; }

    public static InventoryStatusTransition Create(Guid productId, Guid batchId, StockCondition from,
        StockCondition to, BoxQuantity quantity, string sourceReference, DateTimeOffset occurredAt, Guid? id = null)
        => new(id ?? Guid.NewGuid(), productId, batchId, from, to, quantity, sourceReference, occurredAt);

    public IReadOnlyList<StockAdjustment> ToMovements()
        => new StockAdjustment[]
        {
            new(ProductId, BatchId, Quantity, AdjustmentDirection.Decrease, From, SourceReference, OccurredAt),
            new(ProductId, BatchId, Quantity, AdjustmentDirection.Increase, To, SourceReference, OccurredAt)
        };
}
