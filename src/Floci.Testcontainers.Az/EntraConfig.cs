using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Microsoft Entra ID emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithEntra(EntraConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record EntraConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "ENTRA";
}
