using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SecretsManagerConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SecretsManagerConfig();

        Assert.True(config.Enabled);
        Assert.Equal(30, config.RecoveryWindowDays);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SecretsManagerConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SECRETSMANAGER_ENABLED"]);
        Assert.Equal("30", env["FLOCI_SERVICES_SECRETSMANAGER_DEFAULT_RECOVERY_WINDOW_DAYS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new SecretsManagerConfig
        {
            RecoveryWindowDays = 7,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SECRETSMANAGER_ENABLED"]);
        Assert.Equal("7", env["FLOCI_SERVICES_SECRETSMANAGER_DEFAULT_RECOVERY_WINDOW_DAYS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SecretsManagerConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_SECRETSMANAGER_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SECRETSMANAGER_DEFAULT_RECOVERY_WINDOW_DAYS", env.Keys);
    }
}
