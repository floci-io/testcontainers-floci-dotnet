using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AwsConfigConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new AwsConfigConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new AwsConfigConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_CONFIGSERVICE_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new AwsConfigConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_CONFIGSERVICE_ENABLED"]);
        Assert.Single(env);
    }
}
