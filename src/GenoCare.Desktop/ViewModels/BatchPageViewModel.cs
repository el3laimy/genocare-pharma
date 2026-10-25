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

    [ObservableProperty] public partial ProductListItem? SelectedProduct { get; set; }
    [ObservableProperty] public partial string BatchNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial DateTimeOffset? SelectedDate { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsBusy { get; set; }

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

    partial void OnSelectedProductChanged(ProductListItem? value) => _ = LoadBatchesSafelyAsync();

    [RelayCommand]
    private async Task AddBatchAsync()
    {
        await RunAsync(async () =>
        {
            if (SelectedProduct is null) throw new ArgumentException("اختر الصنف أولًا.");
            if (SelectedDate is not DateTimeOffset expiry) throw new ArgumentException("أدخل تاريخ الصلاحية.");
            await batches.CreateAsync(SelectedProduct.Id, BatchNumber, DateOnly.FromDateTime(expiry.DateTime));
            BatchNumber = string.Empty;
            SelectedDate = null;
            await LoadBatchesAsync();
        });
    }

    private async Task LoadBatchesAsync()
    {
        Batches.Clear();
        if (SelectedProduct is null) return;
        foreach (var batch in await batches.ListForProductAsync(SelectedProduct.Id))
        {
            var balance = await inventory.GetBalanceAsync(batch.Id);
            Batches.Add(new BatchListItem(batch.Id, batch.BatchNumber, batch.ExpiryDate,
                balance.Available, balance.Quarantined, balance.Damaged));
        }
    }

    private async Task LoadBatchesSafelyAsync()
    {
        try { await LoadBatchesAsync(); }
        catch (Exception) { ErrorMessage = "تعذر تحميل تشغيلات الصنف المحدد."; }
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
