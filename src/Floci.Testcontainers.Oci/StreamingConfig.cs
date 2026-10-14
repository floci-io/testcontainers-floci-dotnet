using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Streaming emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithStreaming(StreamingConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record StreamingConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "STREAMING";
}
