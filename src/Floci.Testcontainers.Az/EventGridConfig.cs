using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Event Grid emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithEventGrid(EventGridConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record EventGridConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "EVENT_GRID";
}
