using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CurConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CurConfig();

        Assert.True(config.Enabled);
        Assert.Equal("synchronous", config.EmitMode);
        Assert.Equal("floci-cur-staging", config.StagingBucket);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CurConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CUR_ENABLED"]);
        Assert.Equal("synchronous", env["FLOCI_SERVICES_CUR_EMIT_MODE"]);
        Assert.Equal("floci-cur-staging", env["FLOCI_SERVICES_CUR_STAGING_BUCKET"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CurConfig { EmitMode = "daily", StagingBucket = "my-bucket" }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CUR_ENABLED"]);
        Assert.Equal("daily", env["FLOCI_SERVICES_CUR_EMIT_MODE"]);
        Assert.Equal("my-bucket", env["FLOCI_SERVICES_CUR_STAGING_BUCKET"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CurConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CUR_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CUR_EMIT_MODE", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_CUR_STAGING_BUCKET", env.Keys);
    }
}
