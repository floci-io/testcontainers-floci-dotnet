using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AppConfigConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AppConfigConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AppConfigConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_APPCONFIG_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AppConfigConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_APPCONFIG_ENABLED"]);
        Assert.Single(env);
    }
}
