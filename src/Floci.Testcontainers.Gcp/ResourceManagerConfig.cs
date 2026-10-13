using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Resource Manager emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithResourceManager(ResourceManagerConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ResourceManagerConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "RESOURCEMANAGER";
}
