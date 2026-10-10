using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class SesV2ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SesV2Config();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SesV2Config().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_SES_V2_ENABLED"]);
        Assert.Single(env); // only the ENABLED key
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SesV2Config { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_SES_V2_ENABLED"]);
        Assert.Single(env);
    }
}
