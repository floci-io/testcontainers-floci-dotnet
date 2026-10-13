using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Security Token Service emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithSts(StsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record StsConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "STS";
}
