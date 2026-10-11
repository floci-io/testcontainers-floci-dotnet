using System.Linq;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class RdsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new RdsConfig();

        Assert.True(config.Enabled);
        Assert.Equal(7000, config.ProxyBasePort);
        Assert.Equal(10, config.ProxyPortsCount);
        Assert.Equal(7009, config.ProxyMaxPort);
        Assert.Equal("postgres:16-alpine", config.PostgresImage);
        Assert.Equal("mysql:8.0", config.MysqlImage);
        Assert.Equal("mariadb:11", config.MariadbImage);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void IsContainerBasedAndPublishesProxyPortRange()
    {
        var config = new RdsConfig();

        Assert.True(config.RequiresDockerAccess);
        Assert.Equal(Enumerable.Range(7000, 10), config.FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new RdsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_RDS_ENABLED"]);
        Assert.Equal("7000", env["FLOCI_SERVICES_RDS_PROXY_BASE_PORT"]);
        Assert.Equal("7009", env["FLOCI_SERVICES_RDS_PROXY_MAX_PORT"]);
        Assert.Equal("postgres:16-alpine", env["FLOCI_SERVICES_RDS_DEFAULT_POSTGRES_IMAGE"]);
        Assert.Equal("mysql:8.0", env["FLOCI_SERVICES_RDS_DEFAULT_MYSQL_IMAGE"]);
        Assert.Equal("mariadb:11", env["FLOCI_SERVICES_RDS_DEFAULT_MARIADB_IMAGE"]);
        Assert.DoesNotContain("FLOCI_SERVICES_RDS_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new RdsConfig
        {
            ProxyBasePort = 8000,
            ProxyPortsCount = 5,
            PostgresImage = "postgres:15",
            DockerNetwork = "floci-net",
            EndpointHost = "rds.example.com",
        }.BuildEnvironment();

        Assert.Equal("8000", env["FLOCI_SERVICES_RDS_PROXY_BASE_PORT"]);
        Assert.Equal("8004", env["FLOCI_SERVICES_RDS_PROXY_MAX_PORT"]);
        Assert.Equal("postgres:15", env["FLOCI_SERVICES_RDS_DEFAULT_POSTGRES_IMAGE"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_RDS_DOCKER_NETWORK"]);
        Assert.Equal("rds.example.com", env["FLOCI_SERVICES_RDS_ENDPOINT_HOST"]);
    }

    [Theory]
    [InlineData("docker.example", false, "unix:///var/run/docker.sock", "docker.example")]
    [InlineData("docker.example", true, "unix:///var/run/docker.sock", "docker.example")]
    [InlineData(null, true, "unix:///var/run/docker.sock", null)]
    [InlineData(null, false, "tcp://10.0.0.5:2375", "10.0.0.5")]
    [InlineData(null, false, "unix:///var/run/docker.sock", "127.0.0.1")]
    public void PicksTheHostTheTestProcessReaches(string? hostOverride, bool inContainer, string endpoint, string? expected)
    {
        Assert.Equal(expected, FlociBuilder.DockerHost(hostOverride, inContainer, new Uri(endpoint)));
    }

    [Fact]
    public void BuilderLeavesAnExplicitOrDisabledEndpointHostAlone()
    {
        const string key = "FLOCI_SERVICES_RDS_ENDPOINT_HOST";

        Assert.Equal("rds.example.com", new FlociBuilder("floci/floci:latest")
            .WithRds(new RdsConfig { EndpointHost = "rds.example.com" })
            .WithReachableRdsEndpoint().Environments[key]);
        Assert.False(new FlociBuilder("floci/floci:latest")
            .WithRds(new RdsConfig { Enabled = false })
            .WithReachableRdsEndpoint().Environments.ContainsKey(key));
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new RdsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_RDS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_RDS_PROXY_BASE_PORT", env.Keys);
    }
}
