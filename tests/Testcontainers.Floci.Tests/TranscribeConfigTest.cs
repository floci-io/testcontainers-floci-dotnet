using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TranscribeConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new TranscribeConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new TranscribeConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_TRANSCRIBE_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new TranscribeConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_TRANSCRIBE_ENABLED"]);
        Assert.Single(env);
    }
}
