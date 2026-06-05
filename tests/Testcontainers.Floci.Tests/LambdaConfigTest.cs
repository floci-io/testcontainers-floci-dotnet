using System.Linq;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class LambdaConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new LambdaConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Ephemeral);
        Assert.Equal(128, config.MemoryMb);
        Assert.Equal(3, config.TimeoutSeconds);
        Assert.Equal(9200, config.RuntimeApiBasePort);
        Assert.Equal(9209, config.RuntimeApiMaxPort);
        Assert.False(config.ExposeRuntimePorts);
        Assert.False(config.HotReloadEnabled);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void IsContainerBasedButPublishesNoPortsByDefault()
    {
        var config = new LambdaConfig();

        Assert.True(config.RequiresDockerAccess);
        Assert.Empty(config.FixedHostPorts);
    }

    [Fact]
    public void PublishesRuntimePortRangeWhenExposed()
    {
        var config = new LambdaConfig { ExposeRuntimePorts = true };

        Assert.Equal(Enumerable.Range(9200, 10), config.FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new LambdaConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_LAMBDA_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_LAMBDA_EPHEMERAL"]);
        Assert.Equal("128", env["FLOCI_SERVICES_LAMBDA_DEFAULT_MEMORY_MB"]);
        Assert.Equal("3", env["FLOCI_SERVICES_LAMBDA_DEFAULT_TIMEOUT_SECONDS"]);
        Assert.Equal("9200", env["FLOCI_SERVICES_LAMBDA_RUNTIME_API_BASE_PORT"]);
        Assert.Equal("9209", env["FLOCI_SERVICES_LAMBDA_RUNTIME_API_MAX_PORT"]);
        Assert.Equal("1000", env["FLOCI_SERVICES_LAMBDA_POLL_INTERVAL_MS"]);
        Assert.Equal("300", env["FLOCI_SERVICES_LAMBDA_CONTAINER_IDLE_TIMEOUT_SECONDS"]);
        Assert.Equal("1000", env["FLOCI_SERVICES_LAMBDA_REGION_CONCURRENCY_LIMIT"]);
        Assert.Equal("100", env["FLOCI_SERVICES_LAMBDA_UNRESERVED_CONCURRENCY_MIN"]);
        Assert.Equal("false", env["FLOCI_SERVICES_LAMBDA_HOT_RELOAD_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_LAMBDA_HOT_RELOAD_ALLOWED_PATHS", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_LAMBDA_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new LambdaConfig
        {
            Ephemeral = true,
            MemoryMb = 256,
            TimeoutSeconds = 30,
            HotReloadEnabled = true,
            HotReloadAllowedPaths = new[] { "/a", "/b" },
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_LAMBDA_EPHEMERAL"]);
        Assert.Equal("256", env["FLOCI_SERVICES_LAMBDA_DEFAULT_MEMORY_MB"]);
        Assert.Equal("30", env["FLOCI_SERVICES_LAMBDA_DEFAULT_TIMEOUT_SECONDS"]);
        Assert.Equal("true", env["FLOCI_SERVICES_LAMBDA_HOT_RELOAD_ENABLED"]);
        Assert.Equal("/a,/b", env["FLOCI_SERVICES_LAMBDA_HOT_RELOAD_ALLOWED_PATHS"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_LAMBDA_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new LambdaConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_LAMBDA_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_LAMBDA_DEFAULT_MEMORY_MB", env.Keys);
    }
}
