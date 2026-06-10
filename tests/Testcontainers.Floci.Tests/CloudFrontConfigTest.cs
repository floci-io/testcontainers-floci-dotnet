using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudFrontConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudFrontConfig();

        Assert.True(config.Enabled);
        Assert.Equal("cloudfront.net", config.DomainSuffix);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CloudFrontConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDFRONT_ENABLED"]);
        Assert.Equal("cloudfront.net", env["FLOCI_SERVICES_CLOUDFRONT_DOMAIN_SUFFIX"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CloudFrontConfig { DomainSuffix = "example.com" }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDFRONT_ENABLED"]);
        Assert.Equal("example.com", env["FLOCI_SERVICES_CLOUDFRONT_DOMAIN_SUFFIX"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CloudFrontConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDFRONT_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CLOUDFRONT_DOMAIN_SUFFIX", env.Keys);
    }
}
