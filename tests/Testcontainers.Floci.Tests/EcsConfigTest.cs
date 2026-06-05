using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EcsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new EcsConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Equal(512, config.MemoryMb);
        Assert.Equal(256, config.CpuUnits);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void RequiresDockerAccessWhenNotMocked()
    {
        var config = new EcsConfig { Mock = false };

        Assert.True(config.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotRequireDockerAccessWhenMocked()
    {
        var config = new EcsConfig { Mock = true };

        Assert.False(config.RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new EcsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ECS_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_ECS_MOCK"]);
        Assert.Equal("512", env["FLOCI_SERVICES_ECS_DEFAULT_MEMORY_MB"]);
        Assert.Equal("256", env["FLOCI_SERVICES_ECS_DEFAULT_CPU_UNITS"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ECS_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new EcsConfig
        {
            Mock = true,
            MemoryMb = 1024,
            CpuUnits = 512,
            DockerNetwork = "floci-net",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ECS_MOCK"]);
        Assert.Equal("1024", env["FLOCI_SERVICES_ECS_DEFAULT_MEMORY_MB"]);
        Assert.Equal("512", env["FLOCI_SERVICES_ECS_DEFAULT_CPU_UNITS"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_ECS_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new EcsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ECS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ECS_MOCK", env.Keys);
    }
}
