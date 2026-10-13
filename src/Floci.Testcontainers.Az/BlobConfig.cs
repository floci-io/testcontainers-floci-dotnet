using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Blob Storage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithBlob(BlobConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record BlobConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "BLOB";
}
