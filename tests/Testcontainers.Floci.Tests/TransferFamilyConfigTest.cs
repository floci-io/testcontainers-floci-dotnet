using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TransferFamilyConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new TransferFamilyConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new TransferFamilyConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_TRANSFER_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new TransferFamilyConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_TRANSFER_ENABLED"]);
        Assert.Single(env);
    }
}
