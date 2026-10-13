using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure SignalR Service emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithSignalR(SignalRConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record SignalRConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SIGNALR";
}
