using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EksConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new EksConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Equal("k3s", config.Provider);
        Assert.Equal("rancher/k3s:latest", config.DefaultImage);
        Assert.Equal(6500, config.ApiServerBasePort);
        Assert.Equal(10, config.ApiServerPortsCount);
        Assert.Equal(6509, config.ApiServerMaxPort);
        Assert.Equal("host", config.EndpointMode);
        Assert.True(config.IamAuthWebhook);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void RequiresDockerAccessWhenNotMocked()
    {
        Assert.True(new EksConfig { Mock = false }.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotRequireDockerAccessWhenMocked()
    {
        Assert.False(new EksConfig { Mock = true }.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotPublishApiServerPortsOnTheGateway()
    {
        // Floci publishes the spawned k3s API server on the host port directly (like OpenSearch),
        // so the gateway must not bind the API-server range.
        Assert.Empty(new EksConfig().FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new EksConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EKS_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_EKS_MOCK"]);
        Assert.Equal("k3s", env["FLOCI_SERVICES_EKS_PROVIDER"]);
        Assert.Equal("rancher/k3s:latest", env["FLOCI_SERVICES_EKS_DEFAULT_IMAGE"]);
        Assert.Equal("6500", env["FLOCI_SERVICES_EKS_API_SERVER_BASE_PORT"]);
        Assert.Equal("6509", env["FLOCI_SERVICES_EKS_API_SERVER_MAX_PORT"]);
        Assert.Equal("host", env["FLOCI_SERVICES_EKS_ENDPOINT_MODE"]);
        Assert.Equal("true", env["FLOCI_SERVICES_EKS_IAM_AUTH_WEBHOOK"]);
        Assert.DoesNotContain("FLOCI_SERVICES_EKS_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new EksConfig
        {
            Mock = true,
            Provider = "kind",
            DefaultImage = "rancher/k3s:v1.31.0-k3s1",
            ApiServerBasePort = 7000,
            ApiServerPortsCount = 5,
            EndpointMode = "network",
            IamAuthWebhook = false,
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EKS_MOCK"]);
        Assert.Equal("kind", env["FLOCI_SERVICES_EKS_PROVIDER"]);
        Assert.Equal("rancher/k3s:v1.31.0-k3s1", env["FLOCI_SERVICES_EKS_DEFAULT_IMAGE"]);
        Assert.Equal("7000", env["FLOCI_SERVICES_EKS_API_SERVER_BASE_PORT"]);
        Assert.Equal("7004", env["FLOCI_SERVICES_EKS_API_SERVER_MAX_PORT"]);
        Assert.Equal("network", env["FLOCI_SERVICES_EKS_ENDPOINT_MODE"]);
        Assert.Equal("false", env["FLOCI_SERVICES_EKS_IAM_AUTH_WEBHOOK"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_EKS_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new EksConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_EKS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_EKS_MOCK", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_EKS_PROVIDER", env.Keys);
    }
}
