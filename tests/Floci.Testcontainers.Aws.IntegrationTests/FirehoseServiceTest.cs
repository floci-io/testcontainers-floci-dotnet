using System.Net;
using System.Threading.Tasks;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class FirehoseServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithFirehose(new FirehoseConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonKinesisFirehoseClient CreateClient()
    {
        return new AmazonKinesisFirehoseClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonKinesisFirehoseConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task ListDeliveryStreamsSucceeds()
    {
        using var firehose = CreateClient();

        var response = await firehose.ListDeliveryStreamsAsync(new ListDeliveryStreamsRequest());

        Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
    }
}
