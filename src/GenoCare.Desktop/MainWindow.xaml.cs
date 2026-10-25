using Microsoft.UI.Xaml;
using GenoCare.Desktop.Pages;
using GenoCare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace GenoCare.Desktop;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavigationShell.SelectedItem = NavigationShell.MenuItems[0];
    }

    public MainWindow(string startupError) : this()
    {
        NavigationShell.Visibility = Visibility.Collapsed;
        StartupErrorText.Text = startupError;
        StartupErrorPanel.Visibility = Visibility.Visible;
    }

    private void NavigationShell_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        if (item.Tag?.ToString() == "Products")
            ContentFrame.Navigate(typeof(ProductPage), App.Services.GetRequiredService<ProductPageViewModel>());
        else if (item.Tag?.ToString() == "Batches")
            ContentFrame.Navigate(typeof(BatchPage), App.Services.GetRequiredService<BatchPageViewModel>());
    }
}
