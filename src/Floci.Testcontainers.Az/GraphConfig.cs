using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Microsoft Graph emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithGraph(GraphConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GraphConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "GRAPH";
}
