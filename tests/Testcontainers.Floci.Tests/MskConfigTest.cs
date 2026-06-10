using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class MskConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new MskConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Equal("redpandadata/redpanda:latest", config.DefaultImage);
    }

    [Fact]
    public void RequiresDockerAccessWhenNotMocked()
    {
        Assert.True(new MskConfig { Mock = false }.RequiresDockerAccess);
    }

    [Fact]
    public void DoesNotRequireDockerAccessWhenMocked()
    {
        Assert.False(new MskConfig { Mock = true }.RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new MskConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_MSK_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_MSK_MOCK"]);
        Assert.Equal("redpandadata/redpanda:latest", env["FLOCI_SERVICES_MSK_DEFAULT_IMAGE"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new MskConfig { Mock = true, DefaultImage = "redpandadata/redpanda:v24" }
            .BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_MSK_ENABLED"]);
        Assert.Equal("true", env["FLOCI_SERVICES_MSK_MOCK"]);
        Assert.Equal("redpandadata/redpanda:v24", env["FLOCI_SERVICES_MSK_DEFAULT_IMAGE"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new MskConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_MSK_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_MSK_MOCK", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_MSK_DEFAULT_IMAGE", env.Keys);
    }
}
