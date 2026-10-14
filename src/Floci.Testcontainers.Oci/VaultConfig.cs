using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Vault emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociOciBuilder.WithVault(VaultConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record VaultConfig : OciServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "VAULT";
}
