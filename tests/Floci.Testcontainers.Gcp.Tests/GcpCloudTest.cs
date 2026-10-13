using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Gcp;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

public sealed class GcpCloudTest
{
    private const string Image = "floci/floci-gcp:latest";

    /// <summary>Every config with a socket rule, enabled and disabled, mocked and real.</summary>
    public static IEnumerable<object[]> SocketConfigs() => new FlociServiceConfig[]
    {
        new BigQueryConfig(), new BigQueryConfig { Mock = true }, new BigQueryConfig { Enabled = false },
        new CloudRunConfig(), new CloudRunConfig { Mock = true }, new CloudRunConfig { Enabled = false },
        new CloudSqlConfig(), new CloudSqlConfig { Mock = true }, new CloudSqlConfig { Enabled = false },
        new GkeConfig(), new GkeConfig { Mock = true }, new GkeConfig { Mock = true, Enabled = false },
        new KafkaConfig(), new KafkaConfig { Mock = true }, new KafkaConfig { Enabled = false },
        new CloudFunctionsConfig(), new GcsConfig(), new PubSubConfig(),
    }.Select(config => new object[] { config });

    [Fact]
    public void DescribesTheGoogleCloudEmulator()
    {
        var cloud = GcpCloud.Descriptor;

        Assert.Equal("gcp", cloud.Name);
        Assert.Equal(4588, cloud.Port);
        Assert.Equal(FlociGcpBuilder.FlociGcpPort, cloud.Port);
        Assert.Equal("FLOCI_GCP_", cloud.EnvPrefix);
        Assert.Equal("/_floci-gcp/health", cloud.HealthPath);
        Assert.Equal("/_floci-gcp/state/reset", cloud.ResetPath);
        Assert.Equal("FLOCI_GCP_SERVICES_DOCKER_NETWORK", cloud.NetworkEnv);
        Assert.Equal("FLOCI_GCP_DOCKER_RESOURCE_NAMESPACE", cloud.ResourceNamespaceEnv);
        Assert.Equal("FLOCI_GCP_SERVICES_GCS_ENABLED", cloud.ServiceEnv("GCS", "ENABLED"));
        Assert.Equal("QUARKUS_LOG_CATEGORY__IO_FLOCI_GCP__LEVEL", GcpCloud.LogLevelEnv);
    }

    [Fact]
    public void HasTheEmulatorsIdentityDefaults()
    {
        Assert.Equal("floci-local", GcpCloud.DefaultProjectId);
        Assert.Equal("FLOCI_GCP_DEFAULT_PROJECT_ID", GcpCloud.ProjectIdEnv);
    }

    [Fact]
    public void SetsTheDefaultEnvironmentAndNothingElse()
    {
        var env = new FlociGcpBuilder(Image).Environments;

        Assert.Equal(new Dictionary<string, string> { ["FLOCI_GCP_TLS_HTTPS_PORT"] = "0" }, env);
    }

    [Fact]
    public void LetsTheCallerOverrideTheDefaultEnvironment()
    {
        var env = new FlociGcpBuilder(Image).WithEnvironment("FLOCI_GCP_TLS_HTTPS_PORT", "8443").Environments;

        Assert.Equal("8443", env["FLOCI_GCP_TLS_HTTPS_PORT"]);
    }

    [Fact]
    public void SetsProjectTlsAndLogLevel()
    {
        var env = new FlociGcpBuilder(Image).WithProjectId("my-project").WithTls().WithLogLevel("DEBUG").Environments;

        Assert.Equal("my-project", env["FLOCI_GCP_DEFAULT_PROJECT_ID"]);
        Assert.Equal("true", env["FLOCI_GCP_TLS_ENABLED"]);
        Assert.Equal("DEBUG", env["QUARKUS_LOG_CATEGORY__IO_FLOCI_GCP__LEVEL"]);
        Assert.Equal("false", new FlociGcpBuilder(Image).WithTls(false).Environments["FLOCI_GCP_TLS_ENABLED"]);
    }

    [Fact]
    public void WaitsForHttpsOnlyWhenTlsIsEnabledInTheFinalEnvironment()
    {
        var plain = new FlociGcpBuilder(Image);
        var configured = plain.WaitStrategies.Count();

        // Off, by default or explicitly: the configured strategies are left alone.
        Assert.Same(plain, plain.WithTlsReadiness());
        Assert.Equal(configured, plain.WithTls(false).WithTlsReadiness().WaitStrategies.Count());

        // On, through the builder or a raw setting: the HTTPS wait follows the configured ones.
        var tls = plain.WithTls();
        Assert.True(tls.WithTlsReadiness().WaitStrategies.Count() > configured);
        Assert.Equal(tls.WaitStrategies, tls.WithTlsReadiness().WaitStrategies.Take(configured));
        Assert.True(plain.WithEnvironment("FLOCI_GCP_TLS_ENABLED", "true").WithTlsReadiness().WaitStrategies.Count() > configured);

        // The last setting wins.
        Assert.Equal(configured, tls.WithTls(false).WithTlsReadiness().WaitStrategies.Count());
    }

    [Fact]
    public void AReplacedConfigTakesItsSettingsWithIt()
    {
        var real = new FlociGcpBuilder(Image).WithGke(new GkeConfig());
        Assert.Equal("false", real.Environments["FLOCI_GCP_SERVICES_GKE_MOCK"]);

        var disabled = real.WithGke(new GkeConfig { Enabled = false });
        Assert.Equal(
            new[] { "FLOCI_GCP_SERVICES_GKE_ENABLED" },
            disabled.Environments.Keys.Where(key => key.StartsWith("FLOCI_GCP_SERVICES_GKE_", StringComparison.Ordinal)));
        Assert.Equal("0", disabled.WithServiceSettings().Environments["FLOCI_GCP_TLS_HTTPS_PORT"]);

        // What the caller set by hand is not a config's to take.
        var explicitMock = real.WithEnvironment("FLOCI_GCP_SERVICES_GKE_MOCK", "true").WithGke(new GkeConfig { Enabled = false });
        Assert.Equal("true", explicitMock.Environments["FLOCI_GCP_SERVICES_GKE_MOCK"]);
    }

    [Fact]
    public void ToleratesRepeatedApplication()
    {
        var builder = new FlociGcpBuilder(Image)
            .WithGke(new GkeConfig { Mock = true })
            .WithGke(new GkeConfig { Enabled = false })
            .WithGke(new GkeConfig());

        Assert.Equal("true", builder.Environments["FLOCI_GCP_SERVICES_GKE_ENABLED"]);
        Assert.Equal("false", builder.Environments["FLOCI_GCP_SERVICES_GKE_MOCK"]);
    }

    [Theory]
    [MemberData(nameof(SocketConfigs))]
    public void DescriptorAgreesWithEachConfig(FlociServiceConfig config)
    {
        var env = new Dictionary<string, string>(config.BuildEnvironment());
        var only = GcpCloud.Descriptor with
        {
            SocketServices = GcpCloud.Descriptor.SocketServices.Where(svc => svc.Token == config.ServiceToken).ToArray(),
        };

        Assert.Equal(config.Enabled && config.RequiresDockerAccess, only.DockerSocketRequired(env));
    }

    [Fact]
    public void SocketTableListsExactlyTheConfigsThatCanNeedTheSocket()
    {
        var tokens = AllConfigs()
            .Where(config => config is GcpMockableServiceConfig)
            .Select(config => config.ServiceToken)
            .OrderBy(token => token, StringComparer.Ordinal);

        Assert.Equal(new[] { "BIGQUERY", "CLOUDRUN", "CLOUDSQL", "GKE", "KAFKA" }, tokens);
        Assert.Equal(tokens, GcpCloud.Descriptor.SocketServices.Select(svc => svc.Token).OrderBy(token => token, StringComparer.Ordinal));
    }

    [Fact]
    public void EverySocketServiceNeedsTheSocketByDefault()
    {
        // With no setting at all, as the emulator starts by default.
        var empty = new Dictionary<string, string>();

        Assert.All(
            GcpCloud.Descriptor.SocketServices,
            svc => Assert.True((GcpCloud.Descriptor with { SocketServices = new[] { svc } }).DockerSocketRequired(empty)));
    }

    [Fact]
    public void MountsTheSocketByDefaultSinceContainerServicesAreEnabledByDefault()
    {
        Assert.Equal(FlociHost.HostDockerSocket(), new FlociGcpBuilder(Image).DockerSocketMount());
    }

    [Fact]
    public void SkipsTheSocketWhenNoContainerServiceIsEnabled()
    {
        Assert.Null(WithoutSocketServices().DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_GCP_SERVICES_BIGQUERY_MOCK", "false")]
    [InlineData("FLOCI_GCP_SERVICES_CLOUDRUN_ENABLED", "true")]
    [InlineData("FLOCI_GCP_SERVICES_CLOUDSQL_ENABLED", "true")]
    [InlineData("FLOCI_GCP_SERVICES_GKE_MOCK", "false")]
    [InlineData("FLOCI_GCP_SERVICES_KAFKA_MOCK", "false")]
    public void ReadsTheFinalEnvironmentIncludingRawOverrides(string name, string value)
    {
        var builder = WithoutSocketServices().WithEnvironment(name, value);

        Assert.Equal(FlociHost.HostDockerSocket(), builder.DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_GCP_SERVICES_GKE_MOCK", "true")]
    [InlineData("FLOCI_GCP_SERVICES_GKE_ENDPOINT_MODE", "network")]
    [InlineData("FLOCI_GCP_SERVICES_CLOUDFUNCTIONS_ENABLED", "true")]
    [InlineData("FLOCI_GCP_SERVICES_BIGQUERY_DUCK_URL", "http://duck:8080")]
    public void IgnoresRawSettingsThatSpawnNoContainer(string name, string value)
    {
        Assert.Null(WithoutSocketServices().WithEnvironment(name, value).DockerSocketMount());
    }

    [Fact]
    public void BigQueryWithAPreRunningSidecarNeedsNoSocket()
    {
        var builder = WithoutSocketServices()
            .WithBigQuery(new BigQueryConfig())
            .WithEnvironment("FLOCI_GCP_SERVICES_BIGQUERY_DUCK_URL", "http://duck:8080");

        Assert.Null(builder.DockerSocketMount());
        Assert.Equal(FlociHost.HostDockerSocket(), WithoutSocketServices().WithBigQuery(new BigQueryConfig()).DockerSocketMount());
    }

    [Fact]
    public void ExplicitChoicesWinBothWays()
    {
        Assert.Null(new FlociGcpBuilder(Image).WithoutDockerSocket().DockerSocketMount());
        Assert.Equal("/run/user/1000/docker.sock", WithoutSocketServices().WithDockerSocket("/run/user/1000/docker.sock").DockerSocketMount());
    }

    [Fact]
    public void GivesEachContainerAUniqueOverridableResourceNamespace()
    {
        // Build picks the namespace, so two containers from one builder get different ones.
        var builder = new FlociGcpBuilder(Image);
        var first = builder.WithDefaultResourceNamespace().ResourceNamespace;
        var second = builder.WithDefaultResourceNamespace().ResourceNamespace;

        Assert.Null(builder.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", first);
        Assert.NotEqual(first, second);

        // A namespace the caller sets, either way, is kept.
        Assert.Equal("ci-42", builder.WithResourceNamespace("ci-42").WithDefaultResourceNamespace().ResourceNamespace);
        Assert.Equal("ci-43", builder.WithEnvironment("FLOCI_GCP_DOCKER_RESOURCE_NAMESPACE", "ci-43").WithDefaultResourceNamespace().ResourceNamespace);
    }

    internal static IReadOnlyList<FlociServiceConfig> AllConfigs() => typeof(FlociGcpBuilder).Assembly.GetTypes()
        .Where(type => typeof(FlociServiceConfig).IsAssignableFrom(type) && !type.IsAbstract)
        .Select(type => (FlociServiceConfig)Activator.CreateInstance(type)!)
        .ToArray();

    private static FlociGcpBuilder WithoutSocketServices() => new FlociGcpBuilder(Image)
        .WithBigQuery(new BigQueryConfig { Mock = true })
        .WithCloudRun(new CloudRunConfig { Enabled = false })
        .WithCloudSql(new CloudSqlConfig { Enabled = false })
        .WithGke(new GkeConfig { Mock = true })
        .WithKafka(new KafkaConfig { Mock = true });
}
