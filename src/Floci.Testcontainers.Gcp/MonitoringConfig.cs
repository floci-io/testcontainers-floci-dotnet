using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Monitoring emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithMonitoring(MonitoringConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record MonitoringConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "MONITORING";
}
