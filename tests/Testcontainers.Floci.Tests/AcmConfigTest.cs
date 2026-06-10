using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AcmConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AcmConfig();

        Assert.True(config.Enabled);
        Assert.Equal(0, config.ValidationWaitSeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AcmConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ACM_ENABLED"]);
        Assert.Equal("0", env["FLOCI_SERVICES_ACM_VALIDATION_WAIT_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new AcmConfig { ValidationWaitSeconds = 5 }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ACM_ENABLED"]);
        Assert.Equal("5", env["FLOCI_SERVICES_ACM_VALIDATION_WAIT_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AcmConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ACM_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ACM_VALIDATION_WAIT_SECONDS", env.Keys);
    }
}
