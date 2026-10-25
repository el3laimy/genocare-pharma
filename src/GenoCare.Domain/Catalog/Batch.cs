namespace GenoCare.Domain.Catalog;

public sealed class Batch
{
    private Batch(Guid id, Guid productId, string batchNumber, DateOnly expiryDate)
    {
        Id = id;
        ProductId = productId;
        BatchNumber = batchNumber;
        ExpiryDate = expiryDate;
    }

    public Guid Id { get; }
    public Guid ProductId { get; }
    public string BatchNumber { get; }
    public DateOnly ExpiryDate { get; }

    public static Batch Create(Guid productId, string batchNumber, DateOnly expiryDate, Guid? id = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        if (batchNumber.Trim().Length > 100)
            throw new ArgumentException("Batch number cannot exceed 100 characters.", nameof(batchNumber));
        if (expiryDate == default)
            throw new ArgumentException("Expiry date is required.", nameof(expiryDate));

        return new Batch(id ?? Guid.NewGuid(), productId, batchNumber.Trim(), expiryDate);
    }
}
