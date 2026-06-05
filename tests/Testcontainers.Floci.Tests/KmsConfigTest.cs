using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class KmsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new KmsConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new KmsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_KMS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_KMS_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new KmsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_KMS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_KMS_ENABLED", key);
    }
}
