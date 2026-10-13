using System;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// The Azure emulator (<c>floci/floci-az</c>) as a <see cref="CloudDescriptor" />, plus its
/// identity defaults.
/// </summary>
[PublicAPI]
public static class AzCloud
{
    /// <summary>The id of the subscription that always exists in the emulator.</summary>
    public const string DefaultSubscriptionId = "00000000-0000-0000-0000-000000000001";

    /// <summary>The id of the emulator's default Microsoft Entra ID tenant.</summary>
    public const string DefaultTenantId = "00000000-0000-0000-0000-000000000002";

    /// <summary>The name of the default storage account.</summary>
    public const string DefaultAccountName = "devstoreaccount1";

    /// <summary>The well-known development storage key of the default storage account.</summary>
    public const string DefaultAccountKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    /// <summary>The env var that sets the emulator's log level.</summary>
    public const string LogLevelEnv = "QUARKUS_LOG_CATEGORY__IO_FLOCI_AZ__LEVEL";

    /// <summary>
    /// Gets the Azure emulator's descriptor. Its socket services are the configs that can need the
    /// Docker socket; a unit test keeps the two in step.
    /// </summary>
    public static CloudDescriptor Descriptor { get; } = new(
        Name: "az",
        Port: 4577,
        EnvPrefix: "FLOCI_AZ_",
        HealthPath: "/_floci/health",
        ResetPath: "/_admin/reset",
        SocketServices: new[]
        {
            Mockable("ACI", mockedByDefault: true),
            Mockable("ACR"),
            Mockable("AKS"),
            Mockable("CONTAINER_APPS", mockedByDefault: true),
            Mockable("COSMOS") with { Condition = CosmosSpawnsContainers },
            Mockable("EVENT_HUB"),
            Mockable("FUNCTIONS"),
            Mockable("MARIA_DB"),
            Mockable("MYSQL"),
            Mockable("POSTGRES"),
            Mockable("REDIS"),
            Mockable("SERVICE_BUS", mockedByDefault: true),
            new SocketService("SQL") { Condition = SqlSpawnsContainers },
            Mockable("VM", mockedByDefault: true),
        });

    private static SocketService Mockable(string token, bool mockedByDefault = false) =>
        new(token, Mockable: true) { MockSetting = "MOCKED", MockedByDefault = mockedByDefault };

    /// <summary>
    /// Cosmos DB spawns a container only for a Docker-backed API engine, all of which are disabled
    /// by default, and never while engine startup is <c>disabled</c>.
    /// </summary>
    private static bool CosmosSpawnsContainers(Func<string, string?> setting)
    {
        return !Is(setting("ENGINES_STARTUP"), "disabled")
            && (Is(setting("ENGINES_MONGODB_ENABLED"), "true")
                || Is(setting("ENGINES_POSTGRESQL_ENABLED"), "true")
                || Is(setting("ENGINES_CASSANDRA_ENABLED"), "true")
                || Is(setting("ENGINES_GREMLIN_ENABLED"), "true"));
    }

    /// <summary>
    /// Azure SQL spawns a container only with the <c>managed</c> data-plane provider. As in the
    /// emulator, an unset provider follows the deprecated <c>MOCKED</c> flag when present, and
    /// otherwise is <c>managed</c> exactly when the SQL Server EULA is accepted.
    /// </summary>
    private static bool SqlSpawnsContainers(Func<string, string?> setting)
    {
        if (setting("DATA_PLANE_PROVIDER") is { } provider)
        {
            return Is(provider, "managed");
        }

        if (setting("MOCKED") is { } mocked)
        {
            return !Is(mocked, "true");
        }

        return Is(setting("ACCEPT_EULA"), "Y");
    }

    private static bool Is(string? value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
}
