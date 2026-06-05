using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class KinesisServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonKinesisClient CreateClient()
    {
        return new AmazonKinesisClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonKinesisConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsStream()
    {
        using var kinesis = CreateClient();
        const string streamName = "test-stream";

        await kinesis.CreateStreamAsync(new CreateStreamRequest
        {
            StreamName = streamName,
            ShardCount = 1,
        });

        var streams = await kinesis.ListStreamsAsync(new ListStreamsRequest());

        Assert.Contains(streamName, streams.StreamNames);
    }
}
