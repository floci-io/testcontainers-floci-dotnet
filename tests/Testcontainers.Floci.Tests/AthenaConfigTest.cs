using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AthenaConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AthenaConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AthenaConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ATHENA_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_ATHENA_MOCK"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new AthenaConfig { Mock = true }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ATHENA_ENABLED"]);
        Assert.Equal("true", env["FLOCI_SERVICES_ATHENA_MOCK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AthenaConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ATHENA_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ATHENA_MOCK", env.Keys);
    }
}
