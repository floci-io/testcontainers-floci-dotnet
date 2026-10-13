using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Monitor / Log Analytics emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithMonitor(MonitorConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record MonitorConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "MONITOR";
}
