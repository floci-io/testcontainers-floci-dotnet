using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Service Usage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithServiceUsage(ServiceUsageConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ServiceUsageConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SERVICEUSAGE";
}
