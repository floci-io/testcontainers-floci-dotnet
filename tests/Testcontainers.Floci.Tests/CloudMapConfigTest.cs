using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudMapConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudMapConfig();

        Assert.True(config.Enabled);
        Assert.Equal(0, config.OperationCompletionDelaySeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CloudMapConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDMAP_ENABLED"]);
        Assert.Equal("0", env["FLOCI_SERVICES_CLOUDMAP_OPERATION_COMPLETION_DELAY_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CloudMapConfig
        {
            OperationCompletionDelaySeconds = 5,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDMAP_ENABLED"]);
        Assert.Equal("5", env["FLOCI_SERVICES_CLOUDMAP_OPERATION_COMPLETION_DELAY_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CloudMapConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDMAP_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CLOUDMAP_OPERATION_COMPLETION_DELAY_SECONDS", env.Keys);
    }
}
