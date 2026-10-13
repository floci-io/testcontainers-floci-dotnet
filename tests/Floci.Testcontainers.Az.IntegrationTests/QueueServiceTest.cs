using System.Threading.Tasks;
using Azure.Storage.Queues;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class QueueServiceTest : IClassFixture<FlociAzFixture>
{
    private readonly FlociAzContainer _floci;

    public QueueServiceTest(FlociAzFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task SendsAndReceivesAMessage()
    {
        var queue = new QueueClient(_floci.GetStorageConnectionString(), "orders");
        await queue.CreateAsync();
        Assert.StartsWith(_floci.GetQueueEndpoint(), queue.Uri.ToString());

        await queue.SendMessageAsync("hello from floci");

        var received = await queue.ReceiveMessagesAsync(maxMessages: 1);
        var message = Assert.Single(received.Value);
        Assert.Equal("hello from floci", message.Body.ToString());
    }
}
