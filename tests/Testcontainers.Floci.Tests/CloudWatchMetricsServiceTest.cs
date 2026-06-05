using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudWatchMetricsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCloudWatchClient CreateClient()
    {
        return new AmazonCloudWatchClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCloudWatchConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task PutsAndListsMetric()
    {
        using var cloudWatch = CreateClient();
        const string ns = "test/ns";
        const string metricName = "TestMetric";

        await cloudWatch.PutMetricDataAsync(new PutMetricDataRequest
        {
            Namespace = ns,
            MetricData = new List<MetricDatum>
            {
                new() { MetricName = metricName, Value = 1.0 },
            },
        });

        var metrics = await cloudWatch.ListMetricsAsync(new ListMetricsRequest { Namespace = ns });

        Assert.Contains(metrics.Metrics, m => m.MetricName == metricName);
    }
}
