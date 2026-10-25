namespace GenoCare.Domain.Catalog;

public enum SellingUnit
{
    Box = 1
}

public sealed class Product
{
    private Product(Guid id, string internalCode, string arabicName, string? description,
        decimal retailPrice, decimal purchasePrice)
    {
        Id = id;
        InternalCode = internalCode;
        ArabicName = arabicName;
        Description = description;
        CurrentRetailPrice = retailPrice;
        CurrentPurchasePrice = purchasePrice;
    }

    public Guid Id { get; }
    public string InternalCode { get; private set; }
    public string ArabicName { get; private set; }
    public string? Description { get; private set; }
    public decimal CurrentRetailPrice { get; private set; }
    public decimal CurrentPurchasePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public SellingUnit SellingUnit { get; } = SellingUnit.Box;

    public static Product Create(string internalCode, string arabicName, decimal retailPrice,
        decimal purchasePrice, string? description = null, Guid? id = null)
    {
        ValidateText(internalCode, nameof(internalCode), 50);
        ValidateText(arabicName, nameof(arabicName), 200);
        ValidatePrice(retailPrice, nameof(retailPrice));
        ValidatePrice(purchasePrice, nameof(purchasePrice));
        if (description is not null && description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(description));

        return new Product(id ?? Guid.NewGuid(), internalCode.Trim(), arabicName.Trim(), description?.Trim(),
            retailPrice, purchasePrice);
    }

    public void ChangePrices(decimal retailPrice, decimal purchasePrice)
    {
        ValidatePrice(retailPrice, nameof(retailPrice));
        ValidatePrice(purchasePrice, nameof(purchasePrice));
        CurrentRetailPrice = retailPrice;
        CurrentPurchasePrice = purchasePrice;
    }

    public void Rename(string arabicName, string? description = null)
    {
        ValidateText(arabicName, nameof(arabicName), 200);
        if (description is not null && description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(description));
        ArabicName = arabicName.Trim();
        Description = description?.Trim();
    }

    public void SetActive(bool active) => IsActive = active;

    private static void ValidateText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", parameterName);
        if (value.Trim().Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
    }

    private static void ValidatePrice(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Price cannot be negative.");
    }
}
