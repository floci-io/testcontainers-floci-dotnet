using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class PipesConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new PipesConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new PipesConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_PIPES_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new PipesConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_PIPES_ENABLED"]);
        Assert.Single(env);
    }
}
