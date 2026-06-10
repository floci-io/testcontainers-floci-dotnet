using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BcmDataExportsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new BcmDataExportsConfig();

        Assert.True(config.Enabled);
        Assert.Equal("synchronous", config.EmitMode);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new BcmDataExportsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BCM_DATA_EXPORTS_ENABLED"]);
        Assert.Equal("synchronous", env["FLOCI_SERVICES_BCM_DATA_EXPORTS_EMIT_MODE"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new BcmDataExportsConfig { EmitMode = "daily" }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BCM_DATA_EXPORTS_ENABLED"]);
        Assert.Equal("daily", env["FLOCI_SERVICES_BCM_DATA_EXPORTS_EMIT_MODE"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new BcmDataExportsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_BCM_DATA_EXPORTS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_BCM_DATA_EXPORTS_EMIT_MODE", env.Keys);
    }
}
