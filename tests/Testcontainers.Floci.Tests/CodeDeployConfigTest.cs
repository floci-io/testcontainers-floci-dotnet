using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CodeDeployConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CodeDeployConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CodeDeployConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_CODEDEPLOY_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CodeDeployConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_CODEDEPLOY_ENABLED"]);
        Assert.Single(env);
    }
}
