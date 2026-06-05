using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class DynamoDbConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new DynamoDbConfig();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultConfigEmitsEnabledFlag()
    {
        var env = new DynamoDbConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_DYNAMODB_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_DYNAMODB_ENABLED", key);
    }

    [Fact]
    public void DisabledConfigEmitsDisabledFlag()
    {
        var env = new DynamoDbConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_DYNAMODB_ENABLED"]);
        var key = Assert.Single(env.Keys);
        Assert.Equal("FLOCI_SERVICES_DYNAMODB_ENABLED", key);
    }
}
