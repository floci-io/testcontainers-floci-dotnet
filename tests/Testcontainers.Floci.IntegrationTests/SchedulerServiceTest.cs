using System.Threading.Tasks;
using Amazon.Scheduler;
using Amazon.Scheduler.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SchedulerServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSchedulerClient CreateClient()
    {
        return new AmazonSchedulerClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSchedulerConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsScheduleGroup()
    {
        using var scheduler = CreateClient();
        const string groupName = "test-group";

        await scheduler.CreateScheduleGroupAsync(new CreateScheduleGroupRequest { Name = groupName });

        var groups = await scheduler.ListScheduleGroupsAsync(new ListScheduleGroupsRequest());

        Assert.Contains(groups.ScheduleGroups, g => g.Name == groupName);
    }
}
