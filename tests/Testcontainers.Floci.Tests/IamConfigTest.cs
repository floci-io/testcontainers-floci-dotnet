using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class IamConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new IamConfig();

        Assert.True(config.Enabled);
        Assert.False(config.EnforcementEnabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new IamConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_IAM_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_IAM_ENFORCEMENT_ENABLED"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new IamConfig
        {
            EnforcementEnabled = true,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_IAM_ENABLED"]);
        Assert.Equal("true", env["FLOCI_SERVICES_IAM_ENFORCEMENT_ENABLED"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new IamConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_IAM_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_IAM_ENFORCEMENT_ENABLED", env.Keys);
    }
}
