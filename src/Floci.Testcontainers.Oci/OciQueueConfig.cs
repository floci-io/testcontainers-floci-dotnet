using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Queue emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithQueue(OciQueueConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record OciQueueConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "QUEUE";
}
