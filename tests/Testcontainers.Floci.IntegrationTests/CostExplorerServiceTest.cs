using System;
using System.Threading.Tasks;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CostExplorerServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCostExplorerClient CreateClient()
    {
        return new AmazonCostExplorerClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCostExplorerConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task GetsCostAndUsage()
    {
        using var ce = CreateClient();

        var response = await ce.GetCostAndUsageAsync(new GetCostAndUsageRequest
        {
            TimePeriod = new DateInterval
            {
                Start = "2024-01-01",
                End = "2024-02-01",
            },
            Granularity = Granularity.MONTHLY,
            Metrics = new System.Collections.Generic.List<string> { "UnblendedCost" },
        });

        Assert.NotNull(response.ResultsByTime);
    }
}
