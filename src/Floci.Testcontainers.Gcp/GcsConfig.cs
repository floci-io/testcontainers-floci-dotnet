using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Storage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithGcs(GcsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GcsConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "GCS";
}
