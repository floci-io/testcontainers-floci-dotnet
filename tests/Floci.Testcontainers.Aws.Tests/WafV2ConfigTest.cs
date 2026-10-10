using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class WafV2ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new WafV2Config();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new WafV2Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_WAFV2_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_WAFV2_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new WafV2Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_WAFV2_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_WAFV2_ENABLED", key);
    }
}
