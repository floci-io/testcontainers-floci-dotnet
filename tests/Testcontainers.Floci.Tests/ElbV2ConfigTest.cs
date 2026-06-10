using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ElbV2ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new ElbV2Config();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Empty(config.ListenerPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new ElbV2Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ELBV2_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_ELBV2_MOCK"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new ElbV2Config { Mock = true }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ELBV2_ENABLED"]);
        Assert.Equal("true", env["FLOCI_SERVICES_ELBV2_MOCK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new ElbV2Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ELBV2_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ELBV2_MOCK", env.Keys);
    }

    [Fact]
    public void ListenerPortsArePublishedAsFixedHostPorts()
    {
        var config = new ElbV2Config { ListenerPorts = new[] { 8085, 8086 } };

        Assert.Equal(new[] { 8085, 8086 }, config.FixedHostPorts);
    }

    [Fact]
    public void NoListenerPortsMeansNoFixedHostPorts()
    {
        Assert.Empty(new ElbV2Config().FixedHostPorts);
    }
}
