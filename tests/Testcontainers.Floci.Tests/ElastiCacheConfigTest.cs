using System.Linq;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ElastiCacheConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new ElastiCacheConfig();

        Assert.True(config.Enabled);
        Assert.Equal(6379, config.ProxyBasePort);
        Assert.Equal(10, config.ProxyPortsCount);
        Assert.Equal(6388, config.ProxyMaxPort);
        Assert.Equal("valkey/valkey:8", config.Image);
        Assert.Equal("memcached:1.6", config.MemcachedImage);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void IsContainerBasedAndPublishesProxyPortRange()
    {
        var config = new ElastiCacheConfig();

        Assert.True(config.RequiresDockerAccess);
        Assert.Equal(Enumerable.Range(6379, 10), config.FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new ElastiCacheConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ELASTICACHE_ENABLED"]);
        Assert.Equal("6379", env["FLOCI_SERVICES_ELASTICACHE_PROXY_BASE_PORT"]);
        Assert.Equal("6388", env["FLOCI_SERVICES_ELASTICACHE_PROXY_MAX_PORT"]);
        Assert.Equal("valkey/valkey:8", env["FLOCI_SERVICES_ELASTICACHE_DEFAULT_IMAGE"]);
        Assert.Equal("memcached:1.6", env["FLOCI_SERVICES_ELASTICACHE_DEFAULT_MEMCACHED_IMAGE"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ELASTICACHE_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new ElastiCacheConfig
        {
            ProxyBasePort = 7000,
            ProxyPortsCount = 5,
            Image = "valkey/valkey:7",
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("7000", env["FLOCI_SERVICES_ELASTICACHE_PROXY_BASE_PORT"]);
        Assert.Equal("7004", env["FLOCI_SERVICES_ELASTICACHE_PROXY_MAX_PORT"]);
        Assert.Equal("valkey/valkey:7", env["FLOCI_SERVICES_ELASTICACHE_DEFAULT_IMAGE"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_ELASTICACHE_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new ElastiCacheConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ELASTICACHE_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ELASTICACHE_PROXY_BASE_PORT", env.Keys);
    }
}
