using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EventBridgeConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new EventBridgeConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new EventBridgeConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EVENTBRIDGE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_EVENTBRIDGE_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new EventBridgeConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_EVENTBRIDGE_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_EVENTBRIDGE_ENABLED", key);
    }
}
