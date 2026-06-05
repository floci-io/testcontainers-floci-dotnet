using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SqsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSQSClient CreateClient()
    {
        return new AmazonSQSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSQSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsQueue()
    {
        using var sqs = CreateClient();
        const string queueName = "test-queue";

        await sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName });

        var queues = await sqs.ListQueuesAsync(new ListQueuesRequest());

        Assert.Contains(queues.QueueUrls, url => url.Contains(queueName));
    }

    [Fact]
    public async Task SendsAndReceivesMessage()
    {
        using var sqs = CreateClient();
        const string queueName = "test-message-queue";
        const string body = "Hello from Floci SQS!";

        var queueUrl = (await sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName })).QueueUrl;

        await sqs.SendMessageAsync(new SendMessageRequest { QueueUrl = queueUrl, MessageBody = body });

        var received = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 1,
        });

        var message = Assert.Single(received.Messages);
        Assert.Equal(body, message.Body);
    }
}
