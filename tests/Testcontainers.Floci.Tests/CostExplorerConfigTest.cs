using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CostExplorerConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CostExplorerConfig();

        Assert.True(config.Enabled);
        Assert.Equal(0.0, config.CreditUsdMonthly);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CostExplorerConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CE_ENABLED"]);
        Assert.Equal("0", env["FLOCI_SERVICES_CE_CREDIT_USD_MONTHLY"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CostExplorerConfig { CreditUsdMonthly = 100.0 }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CE_ENABLED"]);
        Assert.Equal("100", env["FLOCI_SERVICES_CE_CREDIT_USD_MONTHLY"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CostExplorerConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CE_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CE_CREDIT_USD_MONTHLY", env.Keys);
    }
}
