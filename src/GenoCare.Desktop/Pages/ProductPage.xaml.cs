using GenoCare.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GenoCare.Desktop.Pages;

public sealed partial class ProductPage : Page
{
    public ProductPage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ProductPageViewModel viewModel)
        {
            DataContext = viewModel;
            await viewModel.SearchCommand.ExecuteAsync(null);
        }
    }

    private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ProductPageViewModel vm && ProductList.SelectedItem is ProductListItem item)
            vm.Select(item);
    }
}
