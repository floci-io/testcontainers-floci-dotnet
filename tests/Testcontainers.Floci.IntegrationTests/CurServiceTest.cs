using System.Threading.Tasks;
using Amazon.CostAndUsageReport;
using Amazon.CostAndUsageReport.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CurServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCostAndUsageReportClient CreateClient()
    {
        return new AmazonCostAndUsageReportClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCostAndUsageReportConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task DescribesReportDefinitions()
    {
        using var cur = CreateClient();

        var response = await cur.DescribeReportDefinitionsAsync(new DescribeReportDefinitionsRequest());

        Assert.NotNull(response.ReportDefinitions);
    }
}
