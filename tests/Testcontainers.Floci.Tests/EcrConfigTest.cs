using System.Linq;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EcrConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new EcrConfig();

        Assert.True(config.Enabled);
        Assert.Equal("registry:2", config.RegistryImage);
        Assert.Equal("floci-ecr-registry", config.RegistryContainerName);
        Assert.Equal(5100, config.RegistryBasePort);
        Assert.Equal(10, config.RegistryPortsCount);
        Assert.Equal(5109, config.RegistryMaxPort);
        Assert.False(config.TlsEnabled);
        Assert.Equal("hostname", config.UriStyle);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void IsContainerBasedAndPublishesRegistryPortRange()
    {
        var config = new EcrConfig();

        Assert.True(config.RequiresDockerAccess);
        Assert.Equal(Enumerable.Range(5100, 10), config.FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new EcrConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ECR_ENABLED"]);
        Assert.Equal("registry:2", env["FLOCI_SERVICES_ECR_REGISTRY_IMAGE"]);
        Assert.Equal("floci-ecr-registry", env["FLOCI_SERVICES_ECR_REGISTRY_CONTAINER_NAME"]);
        Assert.Equal("5100", env["FLOCI_SERVICES_ECR_REGISTRY_BASE_PORT"]);
        Assert.Equal("5109", env["FLOCI_SERVICES_ECR_REGISTRY_MAX_PORT"]);
        Assert.Equal("false", env["FLOCI_SERVICES_ECR_TLS_ENABLED"]);
        Assert.Equal("hostname", env["FLOCI_SERVICES_ECR_URI_STYLE"]);
        Assert.Equal("false", env["FLOCI_SERVICES_ECR_KEEP_RUNNING_ON_SHUTDOWN"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ECR_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new EcrConfig
        {
            RegistryBasePort = 5200,
            RegistryPortsCount = 5,
            TlsEnabled = true,
            UriStyle = "path",
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("5200", env["FLOCI_SERVICES_ECR_REGISTRY_BASE_PORT"]);
        Assert.Equal("5204", env["FLOCI_SERVICES_ECR_REGISTRY_MAX_PORT"]);
        Assert.Equal("true", env["FLOCI_SERVICES_ECR_TLS_ENABLED"]);
        Assert.Equal("path", env["FLOCI_SERVICES_ECR_URI_STYLE"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_ECR_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new EcrConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ECR_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ECR_REGISTRY_IMAGE", env.Keys);
    }
}
