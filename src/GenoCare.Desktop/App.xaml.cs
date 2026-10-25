using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace GenoCare.Desktop;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static IServiceProvider Services { get; } = ConfigureServices();

    private Window? _window;

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        return services.BuildServiceProvider();
    }
}
