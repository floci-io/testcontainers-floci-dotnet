using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SchedulerConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SchedulerConfig();

        Assert.True(config.Enabled);
        Assert.True(config.InvocationEnabled);
        Assert.Equal(10L, config.TickIntervalSeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SchedulerConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SCHEDULER_ENABLED"]);
        Assert.Equal("true", env["FLOCI_SERVICES_SCHEDULER_INVOCATION_ENABLED"]);
        Assert.Equal("10", env["FLOCI_SERVICES_SCHEDULER_TICK_INTERVAL_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new SchedulerConfig
        {
            InvocationEnabled = false,
            TickIntervalSeconds = 5,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SCHEDULER_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_SCHEDULER_INVOCATION_ENABLED"]);
        Assert.Equal("5", env["FLOCI_SERVICES_SCHEDULER_TICK_INTERVAL_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SchedulerConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_SCHEDULER_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SCHEDULER_INVOCATION_ENABLED", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SCHEDULER_TICK_INTERVAL_SECONDS", env.Keys);
    }
}
