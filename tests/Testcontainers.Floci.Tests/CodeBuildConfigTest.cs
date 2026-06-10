using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CodeBuildConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CodeBuildConfig();

        Assert.True(config.Enabled);
        Assert.Null(config.DockerNetwork);
    }

    [Fact]
    public void RequiresDockerAccess()
    {
        Assert.True(new CodeBuildConfig().RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CodeBuildConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CODEBUILD_ENABLED"]);
        // DockerNetwork is only emitted when set.
        Assert.DoesNotContain("FLOCI_SERVICES_CODEBUILD_DOCKER_NETWORK", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CodeBuildConfig { DockerNetwork = "floci-net" }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CODEBUILD_ENABLED"]);
        Assert.Equal("floci-net", env["FLOCI_SERVICES_CODEBUILD_DOCKER_NETWORK"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CodeBuildConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CODEBUILD_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CODEBUILD_DOCKER_NETWORK", env.Keys);
    }
}
