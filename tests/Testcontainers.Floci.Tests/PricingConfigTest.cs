using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class PricingConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new PricingConfig();

        Assert.True(config.Enabled);
        Assert.Null(config.SnapshotPath);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new PricingConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_PRICING_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_PRICING_SNAPSHOT_PATH", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsSnapshotPath()
    {
        var env = new PricingConfig { SnapshotPath = "/custom/snapshot" }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_PRICING_ENABLED"]);
        Assert.Equal("/custom/snapshot", env["FLOCI_SERVICES_PRICING_SNAPSHOT_PATH"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new PricingConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_PRICING_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_PRICING_SNAPSHOT_PATH", env.Keys);
    }
}
