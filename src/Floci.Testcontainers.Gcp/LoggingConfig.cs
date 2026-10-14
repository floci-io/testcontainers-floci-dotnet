using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Logging emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithLogging(LoggingConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record LoggingConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "LOGGING";
}
