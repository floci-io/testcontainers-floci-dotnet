using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SsmConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SsmConfig();

        Assert.True(config.Enabled);
        Assert.Equal(5, config.MaxParameterHistory);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SsmConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SSM_ENABLED"]);
        Assert.Equal("5", env["FLOCI_SERVICES_SSM_MAX_PARAMETER_HISTORY"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new SsmConfig
        {
            MaxParameterHistory = 10,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SSM_ENABLED"]);
        Assert.Equal("10", env["FLOCI_SERVICES_SSM_MAX_PARAMETER_HISTORY"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SsmConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_SSM_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SSM_MAX_PARAMETER_HISTORY", env.Keys);
    }
}
