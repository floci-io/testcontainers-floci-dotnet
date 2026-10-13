using System.Threading.Tasks;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

/// <summary>One emulator shared by the tests of a class.</summary>
public sealed class FlociAzFixture : IAsyncLifetime
{
    public FlociAzContainer Floci { get; } = new FlociAzBuilder(TestImages.FlociAz).Build();

    public Task InitializeAsync() => Floci.StartAsync();

    public Task DisposeAsync() => Floci.DisposeAsync().AsTask();
}
