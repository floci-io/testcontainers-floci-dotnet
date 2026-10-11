using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class CoreServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    [Fact]
    public async Task ResetWipesStateWithoutRestarting()
    {
        using var sqs = new AmazonSQSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSQSConfig { ServiceURL = _floci.GetEndpoint(), AuthenticationRegion = _floci.Region });
        await sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = "reset-me" });

        await _floci.ResetAsync();

        var queues = await sqs.ListQueuesAsync(new ListQueuesRequest());
        Assert.Empty(queues.QueueUrls ?? new System.Collections.Generic.List<string>());
    }

    [Fact]
    public void HasAUniqueResourceNamespace()
    {
        Assert.Matches("^tc-[0-9a-f]{8}$", _floci.ResourceNamespace);
    }
}
