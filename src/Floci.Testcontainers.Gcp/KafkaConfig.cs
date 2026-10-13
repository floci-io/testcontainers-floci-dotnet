using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Managed Service for Apache Kafka emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each cluster as a Redpanda sibling container, so it needs the Docker socket unless <see cref="GcpMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociGcpBuilder.WithKafka(KafkaConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record KafkaConfig : GcpMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KAFKA";
}
