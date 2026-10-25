using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenoCare.Application.Catalog;
using GenoCare.Domain.Catalog;
using System.Globalization;

namespace GenoCare.Desktop.ViewModels;

public partial class ProductPageViewModel(ProductCatalogUseCases catalog) : ObservableObject
{
    public ObservableCollection<ProductListItem> Products { get; } = [];

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ProductEditorState Editor { get; set; } = new();

    [ObservableProperty]
    public partial ProductListItem? SelectedProduct { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await RunAsync(async () =>
        {
            Products.Clear();
            foreach (var product in await catalog.SearchAsync(SearchText))
                Products.Add(ProductListItem.From(product));
        });
    }

    [RelayCommand]
    private void NewProduct() => Editor = new ProductEditorState();

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunAsync(async () =>
        {
            var draft = Editor.ToDraft();
            Product product;
            if (Editor.Id is Guid id)
                product = await catalog.UpdateAsync(id, draft);
            else
                product = await catalog.CreateAsync(draft);
            Editor = ProductEditorState.From(product);
            SelectedProduct = ProductListItem.From(product);
            await SearchAsync();
            ErrorMessage = "تم حفظ الصنف بنجاح.";
        });
    }

    [RelayCommand]
    private async Task ToggleActiveAsync()
    {
        if (Editor.Id is not Guid id) return;
        await RunAsync(async () =>
        {
            await catalog.SetActiveAsync(id, !Editor.IsActive);
            Editor.IsActive = !Editor.IsActive;
            await SearchAsync();
        });
    }

    public void Select(ProductListItem? item)
        => Editor = item is null ? new ProductEditorState() : ProductEditorState.From(item);

    partial void OnSelectedProductChanged(ProductListItem? value) => Select(value);

    private async Task RunAsync(Func<Task> operation)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try { await operation(); }
        catch (ArgumentException ex) { ErrorMessage = $"تحقق من البيانات: {ex.Message}"; }
        catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
        catch (Exception) { ErrorMessage = "تعذر إتمام العملية. تحقق من قاعدة البيانات وحاول مرة أخرى."; }
        finally { IsBusy = false; }
    }
}

public sealed record ProductListItem(Guid Id, string InternalCode, string ArabicName, string? Description,
    decimal RetailPrice, decimal PurchasePrice, bool IsActive)
{
    public static ProductListItem From(Product product) => new(product.Id, product.InternalCode, product.ArabicName,
        product.Description, product.CurrentRetailPrice, product.CurrentPurchasePrice, product.IsActive);
}

public sealed class ProductEditorState : ObservableObject
{
    public Guid? Id { get; init; }
    public bool IsExisting => Id.HasValue;
    private string _internalCode = string.Empty;
    private string _arabicName = string.Empty;
    private string _description = string.Empty;
    private string _retailPriceText = string.Empty;
    private string _purchasePriceText = string.Empty;
    private bool _isActive = true;
    public string InternalCode { get => _internalCode; set => SetProperty(ref _internalCode, value); }
    public string ArabicName { get => _arabicName; set => SetProperty(ref _arabicName, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string RetailPriceText { get => _retailPriceText; set => SetProperty(ref _retailPriceText, value); }
    public string PurchasePriceText { get => _purchasePriceText; set => SetProperty(ref _purchasePriceText, value); }
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public ProductDraft ToDraft()
    {
        if (!TryParse(RetailPriceText, out var retailPrice))
            throw new ArgumentException("سعر الجمهور غير صالح.", nameof(RetailPriceText));
        if (!TryParse(PurchasePriceText, out var purchasePrice))
            throw new ArgumentException("سعر الشراء غير صالح.", nameof(PurchasePriceText));
        return new ProductDraft(InternalCode, ArabicName, Description, retailPrice, purchasePrice);
    }

    public static ProductEditorState From(ProductListItem item) => new()
    {
        Id = item.Id, InternalCode = item.InternalCode, ArabicName = item.ArabicName,
        Description = item.Description ?? string.Empty, RetailPriceText = Format(item.RetailPrice),
        PurchasePriceText = Format(item.PurchasePrice), IsActive = item.IsActive
    };

    public static ProductEditorState From(Product product) => new()
    {
        Id = product.Id, InternalCode = product.InternalCode, ArabicName = product.ArabicName,
        Description = product.Description ?? string.Empty, RetailPriceText = Format(product.CurrentRetailPrice),
        PurchasePriceText = Format(product.CurrentPurchasePrice), IsActive = product.IsActive
    };

    private static string Format(decimal value) => value.ToString("G29", CultureInfo.CurrentCulture);

    private static bool TryParse(string value, out decimal result)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
}
