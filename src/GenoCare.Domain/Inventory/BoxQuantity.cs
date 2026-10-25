namespace GenoCare.Domain.Inventory;

public readonly record struct BoxQuantity
{
    private BoxQuantity(int value) => Value = value;
    public int Value { get; }

    public static BoxQuantity Positive(int value) => value > 0
        ? new BoxQuantity(value)
        : throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be positive whole boxes.");
}

public enum StockCondition
{
    Available = 1,
    Quarantined = 2,
    Damaged = 3
}

public enum AdjustmentDirection
{
    Increase = 1,
    Decrease = 2
}
