using GenoCare.Desktop.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GenoCare.Desktop.Pages;

public sealed partial class BatchPage : Page
{
    public BatchPage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is BatchPageViewModel viewModel)
        {
            DataContext = viewModel;
            await viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
