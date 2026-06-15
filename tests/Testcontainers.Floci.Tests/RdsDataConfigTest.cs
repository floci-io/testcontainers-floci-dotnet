using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class RdsDataConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new RdsDataConfig();

        Assert.True(config.Enabled);
        Assert.Equal(180L, config.TransactionTtlSeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new RdsDataConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_RDS_DATA_ENABLED"]);
        Assert.Equal("180", env["FLOCI_SERVICES_RDS_DATA_TRANSACTION_TTL_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new RdsDataConfig
        {
            TransactionTtlSeconds = 60,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_RDS_DATA_ENABLED"]);
        Assert.Equal("60", env["FLOCI_SERVICES_RDS_DATA_TRANSACTION_TTL_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new RdsDataConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_RDS_DATA_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_RDS_DATA_TRANSACTION_TTL_SECONDS", env.Keys);
    }
}
