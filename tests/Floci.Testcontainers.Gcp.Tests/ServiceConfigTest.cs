using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Gcp;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

/// <summary>
/// The enabled switch, and the mock switch where the emulator has one, of every Google Cloud
/// service. Tokens and mock switches are those of the emulator's <c>ServicesConfig</c>.
/// </summary>
public sealed class ServiceConfigTest
{
    /// <summary>Config, service token, and whether the emulator has a mock switch for it.</summary>
    public static IEnumerable<object[]> Services() => new (FlociServiceConfig Config, string Token, bool Mockable)[]
    {
        (new BigQueryConfig(), "BIGQUERY", true),
        (new CloudFunctionsConfig(), "CLOUDFUNCTIONS", false),
        (new CloudRunConfig(), "CLOUDRUN", true),
        (new CloudSqlConfig(), "CLOUDSQL", true),
        (new CloudTasksConfig(), "CLOUDTASKS", false),
        (new ComputeConfig(), "COMPUTE", false),
        (new DatastoreConfig(), "DATASTORE", false),
        (new EventarcConfig(), "EVENTARC", false),
        (new FirebaseAuthConfig(), "FIREBASEAUTH", false),
        (new FirestoreConfig(), "FIRESTORE", false),
        (new GcpIamConfig(), "IAM", false),
        (new GcpKmsConfig(), "KMS", false),
        (new GcpSchedulerConfig(), "SCHEDULER", false),
        (new GcsConfig(), "GCS", false),
        (new GkeConfig(), "GKE", true),
        (new IamCredentialsConfig(), "IAMCREDENTIALS", false),
        (new KafkaConfig(), "KAFKA", true),
        (new LoggingConfig(), "LOGGING", false),
        (new MonitoringConfig(), "MONITORING", false),
        (new PubSubConfig(), "PUBSUB", false),
        (new ResourceManagerConfig(), "RESOURCEMANAGER", false),
        (new SecretManagerConfig(), "SECRETMANAGER", false),
        (new ServiceUsageConfig(), "SERVICEUSAGE", false),
        (new StsConfig(), "STS", false),
    }.Select(svc => new object[] { svc.Config, svc.Token, svc.Mockable });

    [Theory]
    [MemberData(nameof(Services))]
    public void EnabledByDefaultAndNotMocked(FlociServiceConfig config, string token, bool mockable)
    {
        var env = config.BuildEnvironment();

        Assert.True(config.Enabled);
        Assert.Equal("true", env[$"FLOCI_GCP_SERVICES_{token}_ENABLED"]);
        Assert.All(env.Keys, key => Assert.StartsWith($"FLOCI_GCP_SERVICES_{token}_", key));

        if (mockable)
        {
            Assert.False(Assert.IsAssignableFrom<GcpMockableServiceConfig>(config).Mock);
            Assert.Equal("false", env[$"FLOCI_GCP_SERVICES_{token}_MOCK"]);
            Assert.True(config.RequiresDockerAccess);
        }
        else
        {
            Assert.Equal(new[] { $"FLOCI_GCP_SERVICES_{token}_ENABLED" }, env.Keys);
            Assert.False(config.RequiresDockerAccess);
        }
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void DisabledConfigEmitsOnlyTheEnabledFlag(FlociServiceConfig config, string token, bool mockable)
    {
        _ = mockable;
        var env = (config with { Enabled = false }).BuildEnvironment();

        Assert.Equal(new Dictionary<string, string> { [$"FLOCI_GCP_SERVICES_{token}_ENABLED"] = "false" }, env);
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void MockSwitchIsEmittedBothWays(FlociServiceConfig config, string token, bool mockable)
    {
        if (!mockable)
        {
            Assert.IsNotAssignableFrom<GcpMockableServiceConfig>(config);
            return;
        }

        var service = Assert.IsAssignableFrom<GcpMockableServiceConfig>(config);
        Assert.Equal("true", (service with { Mock = true }).BuildEnvironment()[$"FLOCI_GCP_SERVICES_{token}_MOCK"]);
        Assert.Equal("false", (service with { Mock = false }).BuildEnvironment()[$"FLOCI_GCP_SERVICES_{token}_MOCK"]);
        Assert.False((service with { Mock = true }).RequiresDockerAccess);
    }

    [Fact]
    public void CoversEveryConfigInTheAssembly()
    {
        var listed = Services().Select(row => row[0].GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);
        var actual = GcpCloudTest.AllConfigs().Select(config => config.GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);

        Assert.Equal(actual, listed);
    }

    [Fact]
    public void EveryConfigHasABuilderMethodThatAppliesIt()
    {
        foreach (var config in GcpCloudTest.AllConfigs())
        {
            // GcpIamConfig, GcpKmsConfig and GcpSchedulerConfig carry the cloud prefix because the
            // AWS package has an IamConfig, a KmsConfig and a SchedulerConfig; their methods do not.
            var service = config.GetType().Name.Substring(0, config.GetType().Name.Length - "Config".Length);
            var name = "With" + (service.StartsWith("Gcp", StringComparison.Ordinal) ? service.Substring(3) : service);
            var method = typeof(FlociGcpBuilder).GetMethod(name, new[] { config.GetType() });
            Assert.True(method is not null, $"FlociGcpBuilder.{name}({config.GetType().Name}) is missing");

            var disabled = config with { Enabled = false };
            var builder = (FlociGcpBuilder)method!.Invoke(new FlociGcpBuilder("floci/floci-gcp:latest"), new object[] { disabled })!;
            Assert.Equal("false", builder.Environments[$"FLOCI_GCP_SERVICES_{config.ServiceToken}_ENABLED"]);
        }
    }
}
