using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Key Vault emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithKeyVault(KeyVaultConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record KeyVaultConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KEY_VAULT";
}
