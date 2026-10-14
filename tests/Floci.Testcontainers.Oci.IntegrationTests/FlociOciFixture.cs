using System.Threading.Tasks;
using Floci.Testcontainers.Oci;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

/// <summary>One emulator shared by the tests of a class.</summary>
public sealed class FlociOciFixture : IAsyncLifetime
{
    public FlociOciContainer Floci { get; } = new FlociOciBuilder(TestImages.FlociOci).Build();

    public Task InitializeAsync() => Floci.StartAsync();

    public Task DisposeAsync() => Floci.DisposeAsync().AsTask();
}
