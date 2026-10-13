using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Resource Manager (ARM) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithArm(ArmConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ArmConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "ARM";
}
