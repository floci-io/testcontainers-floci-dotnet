using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Compute Engine emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithCompute(ComputeConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ComputeConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "COMPUTE";
}
