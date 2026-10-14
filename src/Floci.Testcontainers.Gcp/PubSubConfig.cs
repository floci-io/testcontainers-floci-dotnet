using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Pub/Sub emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithPubSub(PubSubConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record PubSubConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "PUBSUB";
}
