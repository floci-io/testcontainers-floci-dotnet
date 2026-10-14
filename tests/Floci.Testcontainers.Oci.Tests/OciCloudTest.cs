using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Oci;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

public sealed class OciCloudTest
{
    private const string Image = "floci/floci-oci:latest";

    /// <summary>Every config with a socket rule, enabled and disabled, mocked and real.</summary>
    public static IEnumerable<object[]> SocketConfigs() => new FlociServiceConfig[]
    {
        new OciFunctionsConfig(), new OciFunctionsConfig { Mock = true }, new OciFunctionsConfig { Enabled = false },
        new OkeConfig(), new OkeConfig { Mock = true }, new OkeConfig { Enabled = false }, new OkeConfig { Mock = true, Enabled = false },
        new ObjectStorageConfig(), new OciQueueConfig(),
    }.Select(config => new object[] { config });

    [Fact]
    public void DescribesTheOciEmulator()
    {
        var cloud = OciCloud.Descriptor;

        Assert.Equal("oci", cloud.Name);
        Assert.Equal(4599, cloud.Port);
        Assert.Equal(FlociOciBuilder.FlociOciPort, cloud.Port);
        Assert.Equal("FLOCI_OCI_", cloud.EnvPrefix);
        Assert.Equal("/_floci-oci/health", cloud.HealthPath);
        Assert.Equal("/_floci-oci/state/reset", cloud.ResetPath);
        Assert.Equal("FLOCI_OCI_SERVICES_DOCKER_NETWORK", cloud.NetworkEnv);
        Assert.Equal("FLOCI_OCI_DOCKER_RESOURCE_NAMESPACE", cloud.ResourceNamespaceEnv);
        Assert.Equal("FLOCI_OCI_SERVICES_OBJECTSTORAGE_ENABLED", cloud.ServiceEnv("OBJECTSTORAGE", "ENABLED"));
        Assert.Equal("QUARKUS_LOG_CATEGORY__IO_FLOCI_OCI__LEVEL", OciCloud.LogLevelEnv);
    }

    [Fact]
    public void HasTheEmulatorsIdentityDefaults()
    {
        Assert.Equal("us-ashburn-1", OciCloud.DefaultRegion);
        Assert.Equal("oc1", OciCloud.DefaultRealm);
        Assert.Equal("ocid1.tenancy.oc1..flocilocaltenancy0000000000000000000000000000000000000000", OciCloud.DefaultTenancyId);
        Assert.Equal("floci-local", OciCloud.DefaultNamespace);
        Assert.Equal("ocid1.user.oc1..flocilocaluser0000000000000000000000000000000000000000000000", OciCloud.DefaultUserId);
        Assert.Equal("aa:bb:cc:dd:ee:ff:00:11:22:33:44:55:66:77:88:99", OciCloud.DefaultFingerprint);
    }

    [Fact]
    public void SetsTheDefaultEnvironmentAndNothingElse()
    {
        var env = new FlociOciBuilder(Image).Environments;

        Assert.Equal(new Dictionary<string, string> { ["FLOCI_OCI_TLS_HTTPS_PORT"] = "0" }, env);
    }

    [Fact]
    public void LetsTheCallerOverrideTheDefaultEnvironment()
    {
        var env = new FlociOciBuilder(Image).WithEnvironment("FLOCI_OCI_TLS_HTTPS_PORT", "8443").Environments;

        Assert.Equal("8443", env["FLOCI_OCI_TLS_HTTPS_PORT"]);
    }

    [Fact]
    public void SetsIdentityAndLogLevel()
    {
        var env = new FlociOciBuilder(Image)
            .WithRegion("eu-frankfurt-1")
            .WithTenancyId("ocid1.tenancy.oc1..mine")
            .WithNamespace("my-namespace")
            .WithLogLevel("DEBUG")
            .Environments;

        Assert.Equal("eu-frankfurt-1", env["FLOCI_OCI_DEFAULT_REGION"]);
        Assert.Equal("ocid1.tenancy.oc1..mine", env["FLOCI_OCI_DEFAULT_TENANCY_ID"]);
        Assert.Equal("my-namespace", env["FLOCI_OCI_DEFAULT_NAMESPACE"]);
        Assert.Equal("DEBUG", env["QUARKUS_LOG_CATEGORY__IO_FLOCI_OCI__LEVEL"]);
    }

    [Fact]
    public void KeepsACallerSuppliedPrivateKeyAcrossOtherSettings()
    {
        Assert.Null(new FlociOciBuilder(Image).PrivateKeyPem);
        Assert.Equal("my-key", new FlociOciBuilder(Image).WithPrivateKeyPem("my-key").WithRegion("eu-frankfurt-1").WithoutDockerSocket().PrivateKeyPem);
    }

    [Fact]
    public void WaitsForHttpsOnlyWhenTlsIsEnabledInTheFinalEnvironment()
    {
        var plain = new FlociOciBuilder(Image);
        var configured = plain.WaitStrategies.Count();

        Assert.Same(plain, plain.WithTlsReadiness());
        Assert.Equal(configured, plain.WithEnvironment("FLOCI_OCI_TLS_ENABLED", "false").WithTlsReadiness().WaitStrategies.Count());

        var tls = plain.WithEnvironment("FLOCI_OCI_TLS_ENABLED", "true");
        Assert.True(tls.WithTlsReadiness().WaitStrategies.Count() > configured);
        Assert.Equal(tls.WaitStrategies, tls.WithTlsReadiness().WaitStrategies.Take(configured));
    }

    [Fact]
    public void AReplacedConfigTakesItsSettingsWithIt()
    {
        var real = new FlociOciBuilder(Image).WithOke(new OkeConfig());
        Assert.Equal("false", real.Environments["FLOCI_OCI_SERVICES_OKE_MOCK"]);

        var disabled = real.WithOke(new OkeConfig { Enabled = false });
        Assert.Equal(
            new[] { "FLOCI_OCI_SERVICES_OKE_ENABLED" },
            disabled.Environments.Keys.Where(key => key.StartsWith("FLOCI_OCI_SERVICES_OKE_", StringComparison.Ordinal)));
        Assert.Equal("0", disabled.WithServiceSettings().Environments["FLOCI_OCI_TLS_HTTPS_PORT"]);

        // What the caller set by hand is not a config's to take.
        var explicitMock = real.WithEnvironment("FLOCI_OCI_SERVICES_OKE_MOCK", "true").WithOke(new OkeConfig { Enabled = false });
        Assert.Equal("true", explicitMock.Environments["FLOCI_OCI_SERVICES_OKE_MOCK"]);
    }

    [Fact]
    public void ToleratesRepeatedApplication()
    {
        var builder = new FlociOciBuilder(Image)
            .WithOke(new OkeConfig { Mock = true })
            .WithOke(new OkeConfig { Enabled = false })
            .WithOke(new OkeConfig());

        Assert.Equal("true", builder.Environments["FLOCI_OCI_SERVICES_OKE_ENABLED"]);
        Assert.Equal("false", builder.Environments["FLOCI_OCI_SERVICES_OKE_MOCK"]);
    }

    [Theory]
    [MemberData(nameof(SocketConfigs))]
    public void DescriptorAgreesWithEachConfig(FlociServiceConfig config)
    {
        var env = new Dictionary<string, string>(config.BuildEnvironment());
        var only = OciCloud.Descriptor with
        {
            SocketServices = OciCloud.Descriptor.SocketServices.Where(svc => svc.Token == config.ServiceToken).ToArray(),
        };

        Assert.Equal(config.Enabled && config.RequiresDockerAccess, only.DockerSocketRequired(env));
    }

    [Fact]
    public void SocketTableListsExactlyTheConfigsThatCanNeedTheSocket()
    {
        var tokens = AllConfigs()
            .Where(config => config is OciMockableServiceConfig)
            .Select(config => config.ServiceToken)
            .OrderBy(token => token, StringComparer.Ordinal);

        Assert.Equal(new[] { "FUNCTIONS", "OKE" }, tokens);
        Assert.Equal(tokens, OciCloud.Descriptor.SocketServices.Select(svc => svc.Token).OrderBy(token => token, StringComparer.Ordinal));
    }

    [Fact]
    public void EverySocketServiceNeedsTheSocketByDefault()
    {
        // With no setting at all, as the emulator starts by default.
        var empty = new Dictionary<string, string>();

        Assert.All(
            OciCloud.Descriptor.SocketServices,
            svc => Assert.True((OciCloud.Descriptor with { SocketServices = new[] { svc } }).DockerSocketRequired(empty)));
    }

    [Fact]
    public void MountsTheSocketByDefaultSinceContainerServicesAreEnabledByDefault()
    {
        Assert.Equal(FlociHost.HostDockerSocket(), new FlociOciBuilder(Image).DockerSocketMount());
    }

    [Fact]
    public void SkipsTheSocketWhenNoContainerServiceIsEnabled()
    {
        Assert.Null(WithoutSocketServices().DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_OCI_SERVICES_FUNCTIONS_MOCK", "false")]
    [InlineData("FLOCI_OCI_SERVICES_OKE_ENABLED", "true")]
    public void ReadsTheFinalEnvironmentIncludingRawOverrides(string name, string value)
    {
        var builder = WithoutSocketServices().WithEnvironment(name, value);

        Assert.Equal(FlociHost.HostDockerSocket(), builder.DockerSocketMount());
    }

    [Theory]
    [InlineData("FLOCI_OCI_SERVICES_FUNCTIONS_MOCK", "true")]
    [InlineData("FLOCI_OCI_SERVICES_STREAMING_ENABLED", "true")]
    public void IgnoresRawSettingsThatSpawnNoContainer(string name, string value)
    {
        Assert.Null(WithoutSocketServices().WithEnvironment(name, value).DockerSocketMount());
    }

    [Fact]
    public void ExplicitChoicesWinBothWays()
    {
        Assert.Null(new FlociOciBuilder(Image).WithoutDockerSocket().DockerSocketMount());
        Assert.Equal("/run/user/1000/docker.sock", WithoutSocketServices().WithDockerSocket("/run/user/1000/docker.sock").DockerSocketMount());
    }

    [Fact]
    public void GivesEachContainerAUniqueOverridableResourceNamespace()
    {
        // Build picks the namespace, so two containers from one builder get different ones.
        var builder = new FlociOciBuilder(Image);
        var first = builder.WithDefaultResourceNamespace().ResourceNamespace;
        var second = builder.WithDefaultResourceNamespace().ResourceNamespace;

        Assert.Null(builder.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", first);
        Assert.NotEqual(first, second);

        // A namespace the caller sets, either way, is kept.
        Assert.Equal("ci-42", builder.WithResourceNamespace("ci-42").WithDefaultResourceNamespace().ResourceNamespace);
        Assert.Equal("ci-43", builder.WithEnvironment("FLOCI_OCI_DOCKER_RESOURCE_NAMESPACE", "ci-43").WithDefaultResourceNamespace().ResourceNamespace);
    }

    internal static IReadOnlyList<FlociServiceConfig> AllConfigs() => typeof(FlociOciBuilder).Assembly.GetTypes()
        .Where(type => typeof(FlociServiceConfig).IsAssignableFrom(type) && !type.IsAbstract)
        .Select(type => (FlociServiceConfig)Activator.CreateInstance(type)!)
        .ToArray();

    private static FlociOciBuilder WithoutSocketServices() => new FlociOciBuilder(Image)
        .WithFunctions(new OciFunctionsConfig { Mock = true })
        .WithOke(new OkeConfig { Enabled = false });
}
