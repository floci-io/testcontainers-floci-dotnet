using System.Threading.Tasks;
using Floci.Testcontainers.Gcp;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

public sealed class PubSubServiceTest : IClassFixture<FlociGcpFixture>
{
    private readonly FlociGcpContainer _floci;

    public PubSubServiceTest(FlociGcpFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task PublishesAndPullsAMessage()
    {
        // gRPC in plain text on the emulator port, found through PUBSUB_EMULATOR_HOST, which the
        // fixture sets from GetEmulatorEnv().
        var publisher = await new PublisherServiceApiClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOnly }.BuildAsync();
        var subscriber = await new SubscriberServiceApiClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOnly }.BuildAsync();

        var topic = TopicName.FromProjectTopic(_floci.ProjectId, "orders");
        var subscription = SubscriptionName.FromProjectSubscription(_floci.ProjectId, "orders-sub");
        await publisher.CreateTopicAsync(topic);
        await subscriber.CreateSubscriptionAsync(subscription, topic, pushConfig: null, ackDeadlineSeconds: 30);

        await publisher.PublishAsync(topic, new[] { new PubsubMessage { Data = ByteString.CopyFromUtf8("hello from floci") } });

        var pulled = await subscriber.PullAsync(subscription, maxMessages: 1);
        var received = Assert.Single(pulled.ReceivedMessages);
        Assert.Equal("hello from floci", received.Message.Data.ToStringUtf8());
        await subscriber.AcknowledgeAsync(subscription, new[] { received.AckId });
    }
}
