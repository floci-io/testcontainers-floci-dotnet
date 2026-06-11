using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AutoScalingConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AutoScalingConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AutoScalingConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_AUTOSCALING_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AutoScalingConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_AUTOSCALING_ENABLED"]);
        Assert.Single(env);
    }
}
