using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Key Management emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithKms(OciKmsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record OciKmsConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KMS";
}
