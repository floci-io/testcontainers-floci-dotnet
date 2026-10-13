using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Policy emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithPolicy(PolicyConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record PolicyConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "POLICY";
}
