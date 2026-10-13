using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Communication Services Email emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithEmail(EmailConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record EmailConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "EMAIL";
}
