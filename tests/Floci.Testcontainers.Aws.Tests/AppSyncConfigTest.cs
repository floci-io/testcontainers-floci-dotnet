using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class AppSyncConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AppSyncConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AppSyncConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_APPSYNC_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AppSyncConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_APPSYNC_ENABLED"]);
        Assert.Single(env);
    }
}
