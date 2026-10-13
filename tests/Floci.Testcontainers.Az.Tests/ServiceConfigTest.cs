using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

/// <summary>
/// The enabled switch, and the mock switch where the emulator has one, of every Azure service.
/// Tokens and mock defaults are those of the emulator's <c>ServicesConfig</c>.
/// </summary>
public sealed class ServiceConfigTest
{
    /// <summary>Config, service token, and the emulator's mock default (null: no mock switch).</summary>
    public static IEnumerable<object?[]> Services() => new (FlociServiceConfig Config, string Token, bool? Mocked)[]
    {
        (new AciConfig(), "ACI", true),
        (new AcrConfig(), "ACR", false),
        (new AksConfig(), "AKS", false),
        (new ApimConfig(), "APIM", null),
        (new AppConfigurationConfig(), "APP_CONFIG", null),
        (new ArmConfig(), "ARM", null),
        (new BlobConfig(), "BLOB", null),
        (new ContainerAppsConfig(), "CONTAINER_APPS", true),
        (new CosmosConfig(), "COSMOS", false),
        (new EmailConfig(), "EMAIL", null),
        (new EntraConfig(), "ENTRA", null),
        (new EventGridConfig(), "EVENT_GRID", null),
        (new EventHubConfig(), "EVENT_HUB", false),
        (new FunctionsConfig(), "FUNCTIONS", false),
        (new GraphConfig(), "GRAPH", null),
        (new KeyVaultConfig(), "KEY_VAULT", null),
        (new ManagedIdentityConfig(), "MANAGED_IDENTITY", null),
        (new MariaDbConfig(), "MARIA_DB", false),
        (new MonitorConfig(), "MONITOR", null),
        (new MySqlConfig(), "MYSQL", false),
        (new NetworkConfig(), "NETWORK", null),
        (new PolicyConfig(), "POLICY", null),
        (new PostgresConfig(), "POSTGRES", false),
        (new QueueConfig(), "QUEUE", null),
        (new RedisConfig(), "REDIS", false),
        (new ServiceBusConfig(), "SERVICE_BUS", true),
        (new SignalRConfig(), "SIGNALR", null),
        (new SqlConfig(), "SQL", null),
        (new TableConfig(), "TABLE", null),
        (new VmConfig(), "VM", true),
    }.Select(svc => new object?[] { svc.Config, svc.Token, svc.Mocked });

    [Theory]
    [MemberData(nameof(Services))]
    public void EnabledByDefaultWithTheEmulatorsMockDefault(FlociServiceConfig config, string token, bool? mocked)
    {
        var env = config.BuildEnvironment();

        Assert.True(config.Enabled);
        Assert.Equal("true", env[$"FLOCI_AZ_SERVICES_{token}_ENABLED"]);
        Assert.All(env.Keys, key => Assert.StartsWith($"FLOCI_AZ_SERVICES_{token}_", key));

        if (mocked is null)
        {
            Assert.DoesNotContain($"FLOCI_AZ_SERVICES_{token}_MOCKED", env.Keys);
        }
        else
        {
            Assert.Equal(mocked, Assert.IsAssignableFrom<AzMockableServiceConfig>(config).Mocked);
            Assert.Equal(mocked.Value ? "true" : "false", env[$"FLOCI_AZ_SERVICES_{token}_MOCKED"]);
        }
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void DisabledConfigEmitsOnlyTheEnabledFlag(FlociServiceConfig config, string token, bool? mocked)
    {
        _ = mocked;
        var env = (config with { Enabled = false }).BuildEnvironment();

        Assert.Equal(new Dictionary<string, string> { [$"FLOCI_AZ_SERVICES_{token}_ENABLED"] = "false" }, env);
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void MockSwitchIsEmittedBothWays(FlociServiceConfig config, string token, bool? mocked)
    {
        if (mocked is null || config is not AzMockableServiceConfig mockable)
        {
            return;
        }

        Assert.Equal("true", (mockable with { Mocked = true }).BuildEnvironment()[$"FLOCI_AZ_SERVICES_{token}_MOCKED"]);
        Assert.Equal("false", (mockable with { Mocked = false }).BuildEnvironment()[$"FLOCI_AZ_SERVICES_{token}_MOCKED"]);
        Assert.False((mockable with { Mocked = true }).RequiresDockerAccess);
    }

    [Fact]
    public void CoversEveryConfigInTheAssembly()
    {
        var listed = Services().Select(row => row[0]!.GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);
        var actual = AzCloudTest.AllConfigs().Select(config => config.GetType()).OrderBy(type => type.Name, StringComparer.Ordinal);

        Assert.Equal(actual, listed);
    }

    [Fact]
    public void EveryConfigHasABuilderMethodThatAppliesIt()
    {
        foreach (var config in AzCloudTest.AllConfigs())
        {
            var name = "With" + config.GetType().Name.Substring(0, config.GetType().Name.Length - "Config".Length);
            var method = typeof(FlociAzBuilder).GetMethod(name, new[] { config.GetType() });
            Assert.True(method is not null, $"FlociAzBuilder.{name}({config.GetType().Name}) is missing");

            var disabled = config with { Enabled = false };
            var builder = (FlociAzBuilder)method!.Invoke(new FlociAzBuilder("floci/floci-az:latest"), new object[] { disabled })!;
            Assert.Equal("false", builder.Environments[$"FLOCI_AZ_SERVICES_{config.ServiceToken}_ENABLED"]);
        }
    }
}
