using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class CloudTrailConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudTrailConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new CloudTrailConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDTRAIL_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_CLOUDTRAIL_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new CloudTrailConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDTRAIL_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_CLOUDTRAIL_ENABLED", key);
    }
}
