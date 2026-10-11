using System.Threading.Tasks;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

// ResetAsync is not covered here yet: it needs floci/floci 1.5.29+, newer than TestImages.Floci.
public sealed class CoreServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    [Fact]
    public void HasAUniqueResourceNamespace()
    {
        Assert.Matches("^tc-[0-9a-f]{8}$", _floci.ResourceNamespace);
    }
}
