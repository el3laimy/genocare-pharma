using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenoCare.Application.Catalog;
using GenoCare.Domain.Catalog;

namespace GenoCare.Desktop.ViewModels;

public partial class BatchPageViewModel(ProductCatalogUseCases products, BatchCatalogUseCases batches, IInventoryReadStore inventory)
    : ObservableObject
{
    public ObservableCollection<ProductListItem> Products { get; } = [];
    public ObservableCollection<BatchListItem> Batches { get; } = [];

    [ObservableProperty] private ProductListItem? _selectedProduct;
    [ObservableProperty] private string _batchNumber = string.Empty;
    [ObservableProperty] private DateTimeOffset? _expiryDate;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            Products.Clear();
            foreach (var product in await products.SearchAsync(string.Empty)) Products.Add(ProductListItem.From(product));
            if (SelectedProduct is not null) await LoadBatchesAsync();
        });
    }

    partial void OnSelectedProductChanged(ProductListItem? value)
    {
        if (value is not null) _ = LoadBatchesAsync();
    }

    [RelayCommand]
    private async Task AddBatchAsync()
    {
        await RunAsync(async () =>
        {
            if (SelectedProduct is null) throw new ArgumentException("اختر الصنف أولًا.");
            if (ExpiryDate is not DateTimeOffset expiry) throw new ArgumentException("أدخل تاريخ الصلاحية.");
            await batches.CreateAsync(SelectedProduct.Id, BatchNumber, DateOnly.FromDateTime(expiry.DateTime));
            BatchNumber = string.Empty;
            ExpiryDate = null;
            await LoadBatchesAsync();
        });
    }

    private async Task LoadBatchesAsync()
    {
        if (SelectedProduct is null) return;
        Batches.Clear();
        foreach (var batch in await batches.ListForProductAsync(SelectedProduct.Id))
        {
            var balance = await inventory.GetBalanceAsync(batch.Id);
            Batches.Add(new BatchListItem(batch.Id, batch.BatchNumber, batch.ExpiryDate,
                balance.Available, balance.Quarantined, balance.Damaged));
        }
    }

    private async Task RunAsync(Func<Task> operation)
    {
        IsBusy = true; ErrorMessage = string.Empty;
        try { await operation(); }
        catch (ArgumentException ex) { ErrorMessage = $"تحقق من البيانات: {ex.Message}"; }
        catch (Exception) { ErrorMessage = "تعذر إتمام العملية. تحقق من قاعدة البيانات وحاول مرة أخرى."; }
        finally { IsBusy = false; }
    }
}

public sealed record BatchListItem(Guid Id, string BatchNumber, DateOnly ExpiryDate,
    int Available, int Quarantined, int Damaged);
