using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenoCare.Application.Catalog;
using GenoCare.Domain.Catalog;

namespace GenoCare.Desktop.ViewModels;

public partial class ProductPageViewModel(ProductCatalogUseCases catalog) : ObservableObject
{
    public ObservableCollection<ProductListItem> Products { get; } = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ProductEditorState _editor = new();

    [ObservableProperty]
    private ProductListItem? _selectedProduct;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

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
            if (Editor.Id is Guid id)
                await catalog.UpdateAsync(id, draft);
            else
                await catalog.CreateAsync(draft);
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

public sealed record ProductListItem(Guid Id, string InternalCode, string ArabicName, decimal RetailPrice,
    decimal PurchasePrice, bool IsActive)
{
    public static ProductListItem From(Product product) => new(product.Id, product.InternalCode, product.ArabicName,
        product.CurrentRetailPrice, product.CurrentPurchasePrice, product.IsActive);
}

public sealed class ProductEditorState : ObservableObject
{
    public Guid? Id { get; init; }
    private string _internalCode = string.Empty;
    private string _arabicName = string.Empty;
    private string _description = string.Empty;
    private decimal _retailPrice;
    private decimal _purchasePrice;
    private bool _isActive = true;
    public string InternalCode { get => _internalCode; set => SetProperty(ref _internalCode, value); }
    public string ArabicName { get => _arabicName; set => SetProperty(ref _arabicName, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public decimal RetailPrice { get => _retailPrice; set => SetProperty(ref _retailPrice, value); }
    public decimal PurchasePrice { get => _purchasePrice; set => SetProperty(ref _purchasePrice, value); }
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public ProductDraft ToDraft() => new(InternalCode, ArabicName, Description, RetailPrice, PurchasePrice);

    public static ProductEditorState From(ProductListItem item) => new()
    {
        Id = item.Id, InternalCode = item.InternalCode, ArabicName = item.ArabicName,
        RetailPrice = item.RetailPrice, PurchasePrice = item.PurchasePrice, IsActive = item.IsActive
    };
}
