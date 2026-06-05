using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ApiGatewayConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new ApiGatewayConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new ApiGatewayConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_APIGATEWAY_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_APIGATEWAY_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new ApiGatewayConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_APIGATEWAY_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_APIGATEWAY_ENABLED", key);
    }
}
