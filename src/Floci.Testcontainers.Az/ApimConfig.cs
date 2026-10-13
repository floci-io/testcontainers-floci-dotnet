using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure API Management emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithApim(ApimConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ApimConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APIM";
}
