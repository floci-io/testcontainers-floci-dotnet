using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class AzCloudTest
{
    private const string Image = "floci/floci-az:latest";

    /// <summary>Every config with a socket rule, enabled and disabled, mocked and real.</summary>
    public static IEnumerable<object[]> SocketConfigs() => new FlociServiceConfig[]
    {
        new AciConfig(), new AciConfig { Mocked = false }, new AciConfig { Mocked = false, Enabled = false },
        new AcrConfig(), new AcrConfig { Mocked = true }, new AcrConfig { Enabled = false },
        new AksConfig(), new AksConfig { Mocked = true },
        new ContainerAppsConfig(), new ContainerAppsConfig { Mocked = false },
        new CosmosConfig(), new CosmosConfig { MongoDb = true }, new CosmosConfig { PostgreSql = true },
        new CosmosConfig { Cassandra = true }, new CosmosConfig { Gremlin = true },
        new CosmosConfig { Gremlin = true, Mocked = true }, new CosmosConfig { Gremlin = true, EngineStartup = "disabled" },
        new CosmosConfig { Gremlin = true, Enabled = false },
        new EventHubConfig(), new EventHubConfig { Mocked = true },
        new FunctionsConfig(), new FunctionsConfig { Mocked = true },
        new MariaDbConfig(), new MariaDbConfig { Mocked = true },
        new MySqlConfig(), new MySqlConfig { Mocked = true },
        new PostgresConfig(), new PostgresConfig { Mocked = true },
        new RedisConfig(), new RedisConfig { Mocked = true },
        new ServiceBusConfig(), new ServiceBusConfig { Mocked = false },
        new SqlConfig(), new SqlConfig { AcceptEula = "Y" }, new SqlConfig { AcceptEula = "Y", Enabled = false },
        new SqlConfig { DataPlaneProvider = SqlDataPlaneProvider.Managed },
        new SqlConfig { AcceptEula = "Y", DataPlaneProvider = SqlDataPlaneProvider.External },
        new VmConfig(), new VmConfig { Mocked = false },
        new BlobConfig(),
    }.Select(config => new object[] { config });

    [Fact]
    public void DescribesTheAzureEmulator()
    {
        var cloud = AzCloud.Descriptor;

        Assert.Equal("az", cloud.Name);
        Assert.Equal(4577, cloud.Port);
        Assert.Equal(FlociAzBuilder.FlociAzPort, cloud.Port);
        Assert.Equal("FLOCI_AZ_", cloud.EnvPrefix);
        Assert.Equal("/_floci/health", cloud.HealthPath);
        Assert.Equal("/_admin/reset", cloud.ResetPath);
        Assert.Equal("FLOCI_AZ_SERVICES_DOCKER_NETWORK", cloud.NetworkEnv);
        Assert.Equal("FLOCI_AZ_DOCKER_RESOURCE_NAMESPACE", cloud.ResourceNamespaceEnv);
        Assert.Equal("FLOCI_AZ_SERVICES_BLOB_ENABLED", cloud.ServiceEnv("BLOB", "ENABLED"));
        Assert.Equal("QUARKUS_LOG_CATEGORY__IO_FLOCI_AZ__LEVEL", AzCloud.LogLevelEnv);
    }

    [Fact]
    public void HasTheEmulatorsIdentityDefaults()
    {
        Assert.Equal("00000000-0000-0000-0000-000000000001", AzCloud.DefaultSubscriptionId);
        Assert.Equal("00000000-0000-0000-0000-000000000002", AzCloud.DefaultTenantId);
        Assert.Equal("devstoreaccount1", AzCloud.DefaultAccountName);
        Assert.Equal(
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==",
            AzCloud.DefaultAccountKey);
    }

    [Fact]
    public void SetsTheDefaultEnvironmentAndNothingElse()
    {
        var env = new FlociAzBuilder(Image).Environments;

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["FLOCI_AZ_DOCKER_ENDPOINT_MODE"] = "published",
                ["FLOCI_AZ_TLS_HTTPS_PORT"] = "0",
            },
            env);
    }

    [Fact]
    public void LetsTheCallerOverrideTheDefaultEnvironment()
    {
        var env = new FlociAzBuilder(Image)
            .WithEnvironment("FLOCI_AZ_DOCKER_ENDPOINT_MODE", "auto")
            .WithEnvironment("FLOCI_AZ_TLS_HTTPS_PORT", "8443")
            .Environments;

        Assert.Equal("auto", env["FLOCI_AZ_DOCKER_ENDPOINT_MODE"]);
        Assert.Equal("8443", env["FLOCI_AZ_TLS_HTTPS_PORT"]);
    }

    [Fact]
    public void SetsTlsAndLogLevel()
    {
        var env = new FlociAzBuilder(Image).WithTls().WithLogLevel("DEBUG").Environments;

        Assert.Equal("true", env["FLOCI_AZ_TLS_ENABLED"]);
        Assert.Equal("DEBUG", env["QUARKUS_LOG_CATEGORY__IO_FLOCI_AZ__LEVEL"]);
        Assert.Equal("false", new FlociAzBuilder(Image).WithTls(false).Environments["FLOCI_AZ_TLS_ENABLED"]);
    }

    [Fact]
    public void WaitsForHttpsOnlyWhenTlsIsEnabledInTheFinalEnvironment()
    {
        var plain = new FlociAzBuilder(Image);
        var configured = plain.WaitStrategies.Count();

        // Off, by default or explicitly: the configured strategies are left alone.
        Assert.Same(plain, plain.WithTlsReadiness());
        Assert.Equal(configured, plain.WithTls(false).WithTlsReadiness().WaitStrategies.Count());

        // On, through the builder or a raw setting: the HTTPS wait follows the configured ones.
        var tls = plain.WithTls();
        Assert.True(tls.WithTlsReadiness().WaitStrategies.Count() > configured);
        Assert.Equal(tls.WaitStrategies, tls.WithTlsReadiness().WaitStrategies.Take(configured));
        Assert.True(plain.WithEnvironment("FLOCI_AZ_TLS_ENABLED", "true").WithTlsReadiness().WaitStrategies.Count() > configured);

        // The last setting wins.
        Assert.Equal(configured, tls.WithTls(false).WithTlsReadiness().WaitStrategies.Count());
    }

    [Fact]
    public void ToleratesRepeatedApplication()
    {
        var builder = new FlociAzBuilder(Image)
            .WithServiceBus(new ServiceBusConfig { Mocked = false })
            .WithServiceBus(new ServiceBusConfig { Enabled = false })
            .WithServiceBus(new ServiceBusConfig());

        Assert.Equal("true", builder.Environments["FLOCI_AZ_SERVICES_SERVICE_BUS_ENABLED"]);
        Assert.Equal("true", builder.Environments["FLOCI_AZ_SERVICES_SERVICE_BUS_MOCKED"]);
    }

    [Theory]
    [MemberData(nameof(SocketConfigs))]
    public void DescriptorAgreesWithEachConfig(FlociServiceConfig config)
    {
        var env = new Dictionary<string, string>(config.BuildEnvironment());
        var only = AzCloud.Descriptor with
        {
            SocketServices = AzCloud.Descriptor.SocketServices.Where(svc => svc.Token == config.ServiceToken).ToArray(),
        };

        Assert.Equal(config.Enabled && config.RequiresDockerAccess, only.DockerSocketRequired(env));
    }

    [Fact]
    public void SocketTableListsExactlyTheConfigsThatCanNeedTheSocket()
    {
        var tokens = AllConfigs()
            .Where(config => config is AzMockableServiceConfig or SqlConfig)
            .Select(config => config.ServiceToken)
            .OrderBy(token => token, StringComparer.Ordinal);

        Assert.Equal(tokens, AzCloud.Descriptor.SocketServices.Select(svc => svc.Token).OrderBy(token => token, StringComparer.Ordinal));
    }

    [Fact]
    public void OnlyTheServicesThatAreNotMockedByDefaultNeedTheSocketByDefault()
    {
        // With no setting at all, as the emulator starts by default.
        var empty = new Dictionary<string, string>();
        var fromTable = AzCloud.Descriptor.SocketServices
            .Where(svc => (AzCloud.Descriptor with { SocketServices = new[] { svc } }).DockerSocketRequired(empty))
            .Select(svc => svc.Token);
        var fromConfigs = AllConfigs().Where(config => config.RequiresDockerAccess).Select(config => config.ServiceToken);

        var expected = new[] { "ACR", "AKS", "EVENT_HUB", "FUNCTIONS", "MARIA_DB", "MYSQL", "POSTGRES", "REDIS" };
        Assert.Equal(expected, fromTable.OrderBy(token => token, StringComparer.Ordinal));
        Assert.Equal(expected, fromConfigs.OrderBy(token => token, StringComparer.Ordinal));
    }

    [Fact]
    public void MountsTheSocketByDefaultSinceContainerServicesAreEnabledByDefault()
    {
        Assert.Equal(FlociHost.HostDockerSocket(), new FlociAzBuilder(Image).DockerSocketMount());
    }

    [Fact]
    public void SkipsTheSocketWhenNoContainerServiceIsEnabled()
    {
        Assert.Null(WithoutSocketServices().DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_AZ_SERVICES_ACI_MOCKED", "false")]
    [InlineData("FLOCI_AZ_SERVICES_CONTAINER_APPS_MOCKED", "false")]
    [InlineData("FLOCI_AZ_SERVICES_SERVICE_BUS_MOCKED", "false")]
    [InlineData("FLOCI_AZ_SERVICES_VM_MOCKED", "false")]
    [InlineData("FLOCI_AZ_SERVICES_REDIS_ENABLED", "true")]
    [InlineData("FLOCI_AZ_SERVICES_COSMOS_ENGINES_MONGODB_ENABLED", "true")]
    [InlineData("FLOCI_AZ_SERVICES_SQL_ACCEPT_EULA", "Y")]
    [InlineData("FLOCI_AZ_SERVICES_SQL_DATA_PLANE_PROVIDER", "managed")]
    [InlineData("FLOCI_AZ_SERVICES_SQL_MOCKED", "false")]
    public void ReadsTheFinalEnvironmentIncludingRawOverrides(string name, string value)
    {
        var builder = WithoutSocketServices().WithEnvironment(name, value);

        Assert.Equal(FlociHost.HostDockerSocket(), builder.DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_AZ_SERVICES_SERVICE_BUS_MOCKED", "true")]
    [InlineData("FLOCI_AZ_SERVICES_SQL_DATA_PLANE_PROVIDER", "external")]
    [InlineData("FLOCI_AZ_SERVICES_SQL_MOCKED", "true")]
    [InlineData("FLOCI_AZ_SERVICES_COSMOS_ENGINES_NOSQL_ENABLED", "true")]
    public void IgnoresRawSettingsThatSpawnNoContainer(string name, string value)
    {
        Assert.Null(WithoutSocketServices().WithEnvironment(name, value).DockerSocketMount());
    }

    [Fact]
    public void ExplicitChoicesWinBothWays()
    {
        Assert.Null(new FlociAzBuilder(Image).WithoutDockerSocket().DockerSocketMount());
        Assert.Equal("/run/user/1000/docker.sock", WithoutSocketServices().WithDockerSocket("/run/user/1000/docker.sock").DockerSocketMount());
    }

    [Fact]
    public void GivesEachContainerAUniqueOverridableResourceNamespace()
    {
        // Build picks the namespace, so two containers from one builder get different ones.
        var builder = new FlociAzBuilder(Image);
        var first = builder.WithDefaultResourceNamespace().ResourceNamespace;
        var second = builder.WithDefaultResourceNamespace().ResourceNamespace;

        Assert.Null(builder.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", first);
        Assert.NotEqual(first, second);

        // A namespace the caller sets, either way, is kept.
        Assert.Equal("ci-42", builder.WithResourceNamespace("ci-42").WithDefaultResourceNamespace().ResourceNamespace);
        Assert.Equal("ci-43", builder.WithEnvironment("FLOCI_AZ_DOCKER_RESOURCE_NAMESPACE", "ci-43").WithDefaultResourceNamespace().ResourceNamespace);
    }

    [Theory]
    [InlineData("localhost", "127.0.0.1")]
    [InlineData("LOCALHOST", "127.0.0.1")]
    [InlineData("127.0.0.1", "127.0.0.1")]
    [InlineData("192.168.64.50", "192.168.64.50")]
    [InlineData("::1", "[::1]")]
    [InlineData("does-not-resolve.invalid", "does-not-resolve.invalid")]
    public void StorageEndpointsAddressTheEmulatorByIp(string host, string expected)
    {
        Assert.Equal(expected, FlociAzContainer.IpHost(host));
    }

    internal static IReadOnlyList<FlociServiceConfig> AllConfigs() => typeof(FlociAzBuilder).Assembly.GetTypes()
        .Where(type => typeof(FlociServiceConfig).IsAssignableFrom(type) && !type.IsAbstract)
        .Select(type => (FlociServiceConfig)Activator.CreateInstance(type)!)
        .ToArray();

    private static FlociAzBuilder WithoutSocketServices() => new FlociAzBuilder(Image)
        .WithAcr(new AcrConfig { Enabled = false })
        .WithAks(new AksConfig { Mocked = true })
        .WithEventHub(new EventHubConfig { Enabled = false })
        .WithFunctions(new FunctionsConfig { Mocked = true })
        .WithMariaDb(new MariaDbConfig { Enabled = false })
        .WithMySql(new MySqlConfig { Mocked = true })
        .WithPostgres(new PostgresConfig { Enabled = false })
        .WithRedis(new RedisConfig { Enabled = false });
}
