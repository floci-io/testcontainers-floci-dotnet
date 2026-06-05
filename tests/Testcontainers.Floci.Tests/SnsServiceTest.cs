using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SnsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSimpleNotificationServiceClient CreateClient()
    {
        return new AmazonSimpleNotificationServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSimpleNotificationServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsTopic()
    {
        using var sns = CreateClient();
        const string topicName = "test-topic";

        await sns.CreateTopicAsync(new CreateTopicRequest { Name = topicName });

        var topics = await sns.ListTopicsAsync(new ListTopicsRequest());

        Assert.Contains(topics.Topics, t => t.TopicArn.Contains(topicName));
    }
}
