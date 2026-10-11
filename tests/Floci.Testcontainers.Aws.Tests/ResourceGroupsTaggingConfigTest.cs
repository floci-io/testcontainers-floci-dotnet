using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class ResourceGroupsTaggingConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new ResourceGroupsTaggingConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new ResourceGroupsTaggingConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_TAGGING_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_TAGGING_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new ResourceGroupsTaggingConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_TAGGING_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_TAGGING_ENABLED", key);
    }
}
