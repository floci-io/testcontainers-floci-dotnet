using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudWatchLogsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudWatchLogsConfig();

        Assert.True(config.Enabled);
        Assert.Equal(10000, config.MaxEventsPerQuery);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CloudWatchLogsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDWATCHLOGS_ENABLED"]);
        Assert.Equal("10000", env["FLOCI_SERVICES_CLOUDWATCHLOGS_MAX_EVENTS_PER_QUERY"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CloudWatchLogsConfig
        {
            MaxEventsPerQuery = 5000,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDWATCHLOGS_ENABLED"]);
        Assert.Equal("5000", env["FLOCI_SERVICES_CLOUDWATCHLOGS_MAX_EVENTS_PER_QUERY"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CloudWatchLogsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDWATCHLOGS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CLOUDWATCHLOGS_MAX_EVENTS_PER_QUERY", env.Keys);
    }
}
