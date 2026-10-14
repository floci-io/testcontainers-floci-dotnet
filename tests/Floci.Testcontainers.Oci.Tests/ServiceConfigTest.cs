using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Oci;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

/// <summary>
/// The enabled switch, and the mock switch where the emulator has one, of every OCI
/// service. Tokens and mock switches are those of the emulator's <c>ServicesConfig</c>.
/// </summary>
public sealed class ServiceConfigTest
{
    /// <summary>Config, service token, and whether the emulator has a mock switch for it.</summary>
    public static IEnumerable<object[]> Services() => new (FlociServiceConfig Config, string Token, bool Mockable)[]
    {
        (new IdentityConfig(), "IDENTITY", false),
        (new ObjectStorageConfig(), "OBJECTSTORAGE", false),
        (new OciFunctionsConfig(), "FUNCTIONS", true),
        (new OciKmsConfig(), "KMS", false),
        (new OciQueueConfig(), "QUEUE", false),
        (new OkeConfig(), "OKE", true),
        (new StreamingConfig(), "STREAMING", false),
        (new VaultConfig(), "VAULT", false),
    }.Select(svc => new object[] { svc.Config, svc.Token, svc.Mockable });

    [Theory]
    [MemberData(nameof(Services))]
    public void EnabledByDefaultAndNotMocked(FlociServiceConfig config, string token, bool mockable)
    {
        var env = config.BuildEnvironment();

        Assert.True(config.Enabled);
        Assert.Equal("true", env[$"FLOCI_OCI_SERVICES_{token}_ENABLED"]);
        Assert.All(env.Keys, key => Assert.StartsWith($"FLOCI_OCI_SERVICES_{token}_", key));

        if (mockable)
        {
            Assert.False(Assert.IsAssignableFrom<OciMockableServiceConfig>(config).Mock);
            Assert.Equal("false", env[$"FLOCI_OCI_SERVICES_{token}_MOCK"]);
            Assert.True(config.RequiresDockerAccess);
        }
        else
        {
            Assert.Equal(new[] { $"FLOCI_OCI_SERVICES_{token}_ENABLED" }, env.Keys);
            Assert.False(config.RequiresDockerAccess);
        }
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void DisabledConfigEmitsOnlyTheEnabledFlag(FlociServiceConfig config, string token, bool mockable)
    {
        _ = mockable;
        var env = (config with { Enabled = false }).BuildEnvironment();

        Assert.Equal(new Dictionary<string, string> { [$"FLOCI_OCI_SERVICES_{token}_ENABLED"] = "false" }, env);
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void MockSwitchIsEmittedBothWays(FlociServiceConfig config, string token, bool mockable)
    {
        if (!mockable)
        {
            Assert.IsNotAssignableFrom<OciMockableServiceConfig>(config);
            return;
        }

        var service = Assert.IsAssignableFrom<OciMockableServiceConfig>(config);
        Assert.Equal("true", (service with { Mock = true }).BuildEnvironment()[$"FLOCI_OCI_SERVICES_{token}_MOCK"]);
        Assert.Equal("false", (service with { Mock = false }).BuildEnvironment()[$"FLOCI_OCI_SERVICES_{token}_MOCK"]);
        Assert.False((service with { Mock = true }).RequiresDockerAccess);
    }

    [Fact]
    public void CoversEveryConfigInTheAssembly()
    {
        var listed = Services().Select(row => row[0].GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);
        var actual = OciCloudTest.AllConfigs().Select(config => config.GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);

        Assert.Equal(actual, listed);
    }

    [Fact]
    public void EveryConfigHasABuilderMethodThatAppliesIt()
    {
        foreach (var config in OciCloudTest.AllConfigs())
        {
            // OciFunctionsConfig, OciKmsConfig and OciQueueConfig carry the cloud prefix because the
            // AWS, Azure and GCP packages have types of the same name; their methods do not.
            var service = config.GetType().Name.Substring(0, config.GetType().Name.Length - "Config".Length);
            var name = "With" + (service.StartsWith("Oci", StringComparison.Ordinal) ? service.Substring(3) : service);
            var method = typeof(FlociOciBuilder).GetMethod(name, new[] { config.GetType() });
            Assert.True(method is not null, $"FlociOciBuilder.{name}({config.GetType().Name}) is missing");

            var disabled = config with { Enabled = false };
            var builder = (FlociOciBuilder)method!.Invoke(new FlociOciBuilder("floci/floci-oci:latest"), new object[] { disabled })!;
            Assert.Equal("false", builder.Environments[$"FLOCI_OCI_SERVICES_{config.ServiceToken}_ENABLED"]);
        }
    }
}
