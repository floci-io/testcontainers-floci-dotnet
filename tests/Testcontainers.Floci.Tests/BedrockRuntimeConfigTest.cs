using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BedrockRuntimeConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new BedrockRuntimeConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new BedrockRuntimeConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_BEDROCK_RUNTIME_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new BedrockRuntimeConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_BEDROCK_RUNTIME_ENABLED"]);
        Assert.Single(env);
    }
}
