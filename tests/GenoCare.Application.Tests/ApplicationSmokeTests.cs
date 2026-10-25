using GenoCare.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GenoCare.Application.Tests;

public sealed class ApplicationSmokeTests
{
    [Fact]
    public void Application_registration_is_empty_but_composable()
    {
        var services = new ServiceCollection().AddGenoCareApplication();
        Assert.NotNull(services);
    }
}
