using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudWatchMetricsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudWatchMetricsConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new CloudWatchMetricsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDWATCHMETRICS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_CLOUDWATCHMETRICS_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new CloudWatchMetricsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDWATCHMETRICS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_CLOUDWATCHMETRICS_ENABLED", key);
    }
}
