using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class OpenSearchConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new OpenSearchConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Null(config.DefaultImage);
        Assert.Equal(9400, config.ProxyBasePort);
        Assert.Equal(10, config.ProxyPortsCount);
        Assert.Equal(9409, config.ProxyMaxPort);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void RequiresDockerAccessWhenNotMocked()
    {
        Assert.True(new OpenSearchConfig { Mock = false }.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotRequireDockerAccessWhenMocked()
    {
        Assert.False(new OpenSearchConfig { Mock = true }.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotPublishProxyPortsOnTheGateway()
    {
        // Unlike RDS/Neptune, Floci publishes the spawned OpenSearch sibling on the host port
        // directly, so the gateway must not bind the proxy range (would collide).
        Assert.Empty(new OpenSearchConfig().FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new OpenSearchConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_OPENSEARCH_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_OPENSEARCH_MOCK"]);
        Assert.Equal("9400", env["FLOCI_SERVICES_OPENSEARCH_PROXY_BASE_PORT"]);
        Assert.Equal("9409", env["FLOCI_SERVICES_OPENSEARCH_PROXY_MAX_PORT"]);
        // DefaultImage and DockerNetwork are only emitted when set.
        Assert.DoesNotContain("FLOCI_SERVICES_OPENSEARCH_DEFAULT_IMAGE", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_OPENSEARCH_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new OpenSearchConfig
        {
            Mock = true,
            DefaultImage = "opensearchproject/opensearch:2.18.0",
            ProxyBasePort = 9500,
            ProxyPortsCount = 5,
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_OPENSEARCH_MOCK"]);
        Assert.Equal("opensearchproject/opensearch:2.18.0", env["FLOCI_SERVICES_OPENSEARCH_DEFAULT_IMAGE"]);
        Assert.Equal("9500", env["FLOCI_SERVICES_OPENSEARCH_PROXY_BASE_PORT"]);
        Assert.Equal("9504", env["FLOCI_SERVICES_OPENSEARCH_PROXY_MAX_PORT"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_OPENSEARCH_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new OpenSearchConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_OPENSEARCH_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_OPENSEARCH_MOCK", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_OPENSEARCH_PROXY_BASE_PORT", env.Keys);
    }
}
