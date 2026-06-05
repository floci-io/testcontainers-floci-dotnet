using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class KinesisConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new KinesisConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new KinesisConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_KINESIS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_KINESIS_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new KinesisConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_KINESIS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_KINESIS_ENABLED", key);
    }
}
