using System.Linq;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudWatchLogsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCloudWatchLogsClient CreateClient()
    {
        return new AmazonCloudWatchLogsClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCloudWatchLogsConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndDescribesLogGroup()
    {
        using var logs = CreateClient();
        const string logGroupName = "test-log-group";

        await logs.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = logGroupName });

        var response = await logs.DescribeLogGroupsAsync(new DescribeLogGroupsRequest());

        Assert.Contains(response.LogGroups, g => g.LogGroupName == logGroupName);
    }
}
