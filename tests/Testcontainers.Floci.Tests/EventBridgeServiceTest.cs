using System.Linq;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EventBridgeServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonEventBridgeClient CreateClient()
    {
        return new AmazonEventBridgeClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonEventBridgeConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsEventBus()
    {
        using var eventBridge = CreateClient();
        const string busName = "test-event-bus";

        await eventBridge.CreateEventBusAsync(new CreateEventBusRequest { Name = busName });

        var buses = await eventBridge.ListEventBusesAsync(new ListEventBusesRequest());

        Assert.Contains(buses.EventBuses, b => b.Name == busName);
    }
}
