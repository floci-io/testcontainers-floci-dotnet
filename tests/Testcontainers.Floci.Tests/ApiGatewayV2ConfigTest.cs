using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ApiGatewayV2ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new ApiGatewayV2Config();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new ApiGatewayV2Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_APIGATEWAYV2_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_APIGATEWAYV2_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new ApiGatewayV2Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_APIGATEWAYV2_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_APIGATEWAYV2_ENABLED", key);
    }
}
