using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure App Configuration emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithAppConfiguration(AppConfigurationConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record AppConfigurationConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APP_CONFIG";
}
