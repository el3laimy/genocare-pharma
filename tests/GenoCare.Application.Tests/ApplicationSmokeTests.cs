using GenoCare.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GenoCare.Application.Tests;

public sealed class ApplicationSmokeTests
{
    [Fact]
    public void ApplicationRegistrationIsEmptyButComposable()
    {
        var services = new ServiceCollection().AddGenoCareApplication();
        Assert.NotNull(services);
    }
}
