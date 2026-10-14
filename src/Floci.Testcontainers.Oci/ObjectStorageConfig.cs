using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Object Storage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithObjectStorage(ObjectStorageConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ObjectStorageConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "OBJECTSTORAGE";
}
