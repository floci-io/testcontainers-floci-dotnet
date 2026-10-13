using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class CosmosConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CosmosConfig();

        Assert.True(config.Enabled);
        Assert.False(config.Mocked);
        Assert.Equal("on-demand", config.EngineStartup);
        Assert.False(config.MongoDb);
        Assert.False(config.PostgreSql);
        Assert.False(config.Cassandra);
        Assert.False(config.Gremlin);
        Assert.False(config.RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CosmosConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_ENABLED"]);
        Assert.Equal("false", env["FLOCI_AZ_SERVICES_COSMOS_MOCKED"]);
        Assert.Equal("on-demand", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_STARTUP"]);
        Assert.Equal("false", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_MONGODB_ENABLED"]);
        Assert.Equal("false", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_POSTGRESQL_ENABLED"]);
        Assert.Equal("false", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_CASSANDRA_ENABLED"]);
        Assert.Equal("false", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_GREMLIN_ENABLED"]);
        Assert.Equal(7, env.Count);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CosmosConfig
        {
            Mocked = true,
            EngineStartup = "disabled",
            MongoDb = true,
            PostgreSql = true,
            Cassandra = true,
            Gremlin = true,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_MOCKED"]);
        Assert.Equal("disabled", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_STARTUP"]);
        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_MONGODB_ENABLED"]);
        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_POSTGRESQL_ENABLED"]);
        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_CASSANDRA_ENABLED"]);
        Assert.Equal("true", env["FLOCI_AZ_SERVICES_COSMOS_ENGINES_GREMLIN_ENABLED"]);
    }

    [Fact]
    public void RequiresDockerAccessOnlyForAStartableDockerBackedEngine()
    {
        Assert.True(new CosmosConfig { MongoDb = true }.RequiresDockerAccess);
        Assert.True(new CosmosConfig { PostgreSql = true }.RequiresDockerAccess);
        Assert.True(new CosmosConfig { Cassandra = true }.RequiresDockerAccess);
        Assert.True(new CosmosConfig { Gremlin = true }.RequiresDockerAccess);
        Assert.False(new CosmosConfig { Gremlin = true, Mocked = true }.RequiresDockerAccess);
        Assert.False(new CosmosConfig { Gremlin = true, EngineStartup = "disabled" }.RequiresDockerAccess);
    }
}
