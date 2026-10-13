using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Table Storage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithTable(TableConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record TableConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "TABLE";
}
