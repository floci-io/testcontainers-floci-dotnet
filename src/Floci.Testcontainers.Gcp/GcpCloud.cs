using System;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// The Google Cloud emulator (<c>floci/floci-gcp</c>) as a <see cref="CloudDescriptor" />, plus its
/// identity defaults.
/// </summary>
[PublicAPI]
public static class GcpCloud
{
    /// <summary>The id of the project used when a request names none. Any project id is accepted.</summary>
    public const string DefaultProjectId = "floci-local";

    /// <summary>The env var that sets the default project id.</summary>
    public const string ProjectIdEnv = "FLOCI_GCP_DEFAULT_PROJECT_ID";

    /// <summary>The env var that sets the emulator's log level.</summary>
    public const string LogLevelEnv = "QUARKUS_LOG_CATEGORY__IO_FLOCI_GCP__LEVEL";

    /// <summary>
    /// Gets the Google Cloud emulator's descriptor. Its socket services are the configs that can
    /// need the Docker socket; a unit test keeps the two in step.
    /// </summary>
    public static CloudDescriptor Descriptor { get; } = new(
        Name: "gcp",
        Port: 4588,
        EnvPrefix: "FLOCI_GCP_",
        HealthPath: "/_floci-gcp/health",
        ResetPath: "/_floci-gcp/state/reset",
        SocketServices: new[]
        {
            new SocketService("BIGQUERY", Mockable: true) { Condition = BigQuerySpawnsContainers },
            new SocketService("CLOUDRUN", Mockable: true),
            new SocketService("CLOUDSQL", Mockable: true),
            new SocketService("GKE", Mockable: true),
            new SocketService("KAFKA", Mockable: true),
        });

    /// <summary>
    /// BigQuery starts its floci-duck sidecar only when no pre-running one is configured
    /// (<c>FLOCI_GCP_SERVICES_BIGQUERY_DUCK_URL</c>).
    /// </summary>
    private static bool BigQuerySpawnsContainers(Func<string, string?> setting)
    {
        return string.IsNullOrWhiteSpace(setting("DUCK_URL"));
    }
}
