using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BatchConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new BatchConfig();

        Assert.True(config.Enabled);
        Assert.Equal("immediate", config.RunnerMode);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void ImmediateModeDoesNotRequireDockerAccess()
    {
        var config = new BatchConfig { RunnerMode = "immediate" };

        Assert.False(config.RequiresDockerAccess);
    }

    [Fact]
    public void NonImmediateModeRequiresDockerAccess()
    {
        var config = new BatchConfig { RunnerMode = "docker" };

        Assert.True(config.RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new BatchConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BATCH_ENABLED"]);
        Assert.Equal("immediate", env["FLOCI_SERVICES_BATCH_RUNNER_MODE"]);
        Assert.DoesNotContain("FLOCI_SERVICES_BATCH_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new BatchConfig
        {
            RunnerMode = "docker",
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BATCH_ENABLED"]);
        Assert.Equal("docker", env["FLOCI_SERVICES_BATCH_RUNNER_MODE"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_BATCH_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DockerNetworkNotEmittedWhenNull()
    {
        var env = new BatchConfig { DockerNetwork = null }.BuildEnvironment();

        Assert.DoesNotContain("FLOCI_SERVICES_BATCH_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void DockerNetworkNotEmittedWhenEmpty()
    {
        var env = new BatchConfig { DockerNetwork = "" }.BuildEnvironment();

        Assert.DoesNotContain("FLOCI_SERVICES_BATCH_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new BatchConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_BATCH_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_BATCH_RUNNER_MODE", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_BATCH_DOCKER_NETWORK", env.Keys);
    }
}
