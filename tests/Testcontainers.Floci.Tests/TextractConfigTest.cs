using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TextractConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new TextractConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new TextractConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_TEXTRACT_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new TextractConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_TEXTRACT_ENABLED"]);
        Assert.Single(env);
    }
}
