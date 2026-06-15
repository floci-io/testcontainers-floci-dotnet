using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EmrConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new EmrConfig();

        Assert.True(config.Enabled);
        Assert.Equal("emr-7.5.0", config.DefaultReleaseLabel);
        Assert.Equal(0, config.ClusterStartupDelaySeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new EmrConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EMR_ENABLED"]);
        Assert.Equal("emr-7.5.0", env["FLOCI_SERVICES_EMR_DEFAULT_RELEASE_LABEL"]);
        Assert.Equal("0", env["FLOCI_SERVICES_EMR_CLUSTER_STARTUP_DELAY_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new EmrConfig
        {
            DefaultReleaseLabel = "emr-6.15.0",
            ClusterStartupDelaySeconds = 10,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EMR_ENABLED"]);
        Assert.Equal("emr-6.15.0", env["FLOCI_SERVICES_EMR_DEFAULT_RELEASE_LABEL"]);
        Assert.Equal("10", env["FLOCI_SERVICES_EMR_CLUSTER_STARTUP_DELAY_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new EmrConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_EMR_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_EMR_DEFAULT_RELEASE_LABEL", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_EMR_CLUSTER_STARTUP_DELAY_SECONDS", env.Keys);
    }
}
