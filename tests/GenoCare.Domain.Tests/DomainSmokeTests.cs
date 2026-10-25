using GenoCare.Domain;
using Xunit;

namespace GenoCare.Domain.Tests;

public sealed class DomainSmokeTests
{
    [Fact]
    public void DomainAssemblyIsAvailable() => Assert.NotNull(new DomainMarker());
}
