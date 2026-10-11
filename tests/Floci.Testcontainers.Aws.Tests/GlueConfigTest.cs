using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class GlueConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new GlueConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new GlueConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_GLUE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_GLUE_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new GlueConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_GLUE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_GLUE_ENABLED", key);
    }
}
