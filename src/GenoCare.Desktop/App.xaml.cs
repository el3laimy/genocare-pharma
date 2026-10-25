using Microsoft.Extensions.DependencyInjection;
using GenoCare.Application;
using GenoCare.Infrastructure;
using GenoCare.Infrastructure.Persistence;
using GenoCare.Desktop.ViewModels;
using Microsoft.UI.Xaml;

namespace GenoCare.Desktop;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static ServiceProvider Services { get; private set; } = new ServiceCollection().BuildServiceProvider();

    private Window? _window;

    public App() => InitializeComponent();

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Services = ConfigureServices();
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            _window = Services.GetRequiredService<MainWindow>();
        }
        catch (Exception exception)
        {
            _window = new MainWindow($"تعذر تشغيل قاعدة البيانات المحلية. تحقق من صلاحيات مجلد بيانات التطبيق.\n{exception.Message}");
        }
        _window.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGenoCareApplication();
        services.AddGenoCareInfrastructure(GenoCareDatabasePaths.GetDefaultPath());
        services.AddTransient<ProductPageViewModel>();
        services.AddTransient<BatchPageViewModel>();
        services.AddSingleton<MainWindow>();
        return services.BuildServiceProvider();
    }
}
