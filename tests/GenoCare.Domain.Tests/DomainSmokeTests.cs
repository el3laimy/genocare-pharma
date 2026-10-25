using GenoCare.Domain;

namespace GenoCare.Domain.Tests;

public sealed class DomainSmokeTests
{
    [Fact]
    public void Domain_assembly_is_available() => Assert.NotNull(new DomainMarker());
}
