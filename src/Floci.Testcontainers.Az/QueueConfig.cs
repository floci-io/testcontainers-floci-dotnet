using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Queue Storage emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociAzBuilder.WithQueue(QueueConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record QueueConfig : AzServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "QUEUE";
}
