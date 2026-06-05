using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class StepFunctionsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new StepFunctionsConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new StepFunctionsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_STEPFUNCTIONS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_STEPFUNCTIONS_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new StepFunctionsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_STEPFUNCTIONS_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_STEPFUNCTIONS_ENABLED", key);
    }
}
