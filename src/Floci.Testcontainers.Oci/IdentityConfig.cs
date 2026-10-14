using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Identity and Access Management emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithIdentity(IdentityConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record IdentityConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "IDENTITY";
}
