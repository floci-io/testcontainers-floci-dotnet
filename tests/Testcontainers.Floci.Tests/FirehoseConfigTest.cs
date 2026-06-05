using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class FirehoseConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new FirehoseConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new FirehoseConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_FIREHOSE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_FIREHOSE_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new FirehoseConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_FIREHOSE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_FIREHOSE_ENABLED", key);
    }
}
