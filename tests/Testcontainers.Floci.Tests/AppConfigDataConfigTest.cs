using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AppConfigDataConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AppConfigDataConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AppConfigDataConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_APPCONFIGDATA_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AppConfigDataConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_APPCONFIGDATA_ENABLED"]);
        Assert.Single(env);
    }
}
