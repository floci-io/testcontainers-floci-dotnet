using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Virtual Network emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithNetwork(NetworkConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record NetworkConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "NETWORK";
}
