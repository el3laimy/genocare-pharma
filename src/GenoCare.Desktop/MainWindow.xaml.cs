using Microsoft.UI.Xaml;
using GenoCare.Desktop.Pages;
using GenoCare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace GenoCare.Desktop;

public sealed partial class MainWindow : Window
{
    private bool _startupFailure;
    private bool _navigationInitialized;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(string startupError) : this()
    {
        _startupFailure = true;
        NavigationShell.Visibility = Visibility.Collapsed;
        StartupErrorText.Text = startupError;
        StartupErrorPanel.Visibility = Visibility.Visible;
    }

    private void NavigationShell_Loaded(object sender, RoutedEventArgs e)
    {
        if (_startupFailure || _navigationInitialized) return;
        _navigationInitialized = true;
        NavigationShell.SelectedItem = NavigationShell.MenuItems[0];
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
