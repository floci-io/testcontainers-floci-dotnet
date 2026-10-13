using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Managed Identity emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithManagedIdentity(ManagedIdentityConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ManagedIdentityConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "MANAGED_IDENTITY";
}
