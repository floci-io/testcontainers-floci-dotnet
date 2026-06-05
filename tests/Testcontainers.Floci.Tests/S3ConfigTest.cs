using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class S3ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new S3Config();

        Assert.True(config.Enabled);
        Assert.Equal(3600, config.PresignExpirySeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new S3Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_S3_ENABLED"]);
        Assert.Equal("3600", env["FLOCI_SERVICES_S3_DEFAULT_PRESIGN_EXPIRY_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new S3Config
        {
            PresignExpirySeconds = 7200,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_S3_ENABLED"]);
        Assert.Equal("7200", env["FLOCI_SERVICES_S3_DEFAULT_PRESIGN_EXPIRY_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new S3Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_S3_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_S3_DEFAULT_PRESIGN_EXPIRY_SECONDS", env.Keys);
    }
}
