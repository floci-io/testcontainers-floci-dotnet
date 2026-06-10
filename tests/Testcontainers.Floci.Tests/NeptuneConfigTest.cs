using System.Linq;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class NeptuneConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new NeptuneConfig();

        Assert.True(config.Enabled);
        Assert.Equal(8182, config.ProxyBasePort);
        Assert.Equal(101, config.ProxyPortsCount);
        Assert.Equal(8282, config.ProxyMaxPort);
        Assert.Equal("tinkerpop/gremlin-server:3.7.3", config.DefaultImage);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void RequiresDockerAccess()
    {
        Assert.True(new NeptuneConfig().RequiresDockerAccess);
    }

    [Fact]
    public void PublishesProxyPortRangeOnTheGateway()
    {
        var config = new NeptuneConfig { ProxyBasePort = 8182, ProxyPortsCount = 3 };

        Assert.Equal(new[] { 8182, 8183, 8184 }, config.FixedHostPorts.OrderBy(p => p));
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new NeptuneConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_NEPTUNE_ENABLED"]);
        Assert.Equal("8182", env["FLOCI_SERVICES_NEPTUNE_PROXY_BASE_PORT"]);
        Assert.Equal("8282", env["FLOCI_SERVICES_NEPTUNE_PROXY_MAX_PORT"]);
        Assert.Equal("tinkerpop/gremlin-server:3.7.3", env["FLOCI_SERVICES_NEPTUNE_DEFAULT_IMAGE"]);
        Assert.DoesNotContain("FLOCI_SERVICES_NEPTUNE_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new NeptuneConfig
        {
            ProxyBasePort = 9000,
            ProxyPortsCount = 5,
            DefaultImage = "tinkerpop/gremlin-server:3.7.2",
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("9000", env["FLOCI_SERVICES_NEPTUNE_PROXY_BASE_PORT"]);
        Assert.Equal("9004", env["FLOCI_SERVICES_NEPTUNE_PROXY_MAX_PORT"]);
        Assert.Equal("tinkerpop/gremlin-server:3.7.2", env["FLOCI_SERVICES_NEPTUNE_DEFAULT_IMAGE"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_NEPTUNE_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new NeptuneConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_NEPTUNE_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_NEPTUNE_PROXY_BASE_PORT", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_NEPTUNE_DEFAULT_IMAGE", env.Keys);
    }
}
