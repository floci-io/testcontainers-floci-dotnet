using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's BigQuery emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs queries in a floci-duck sibling container, started on the first query, so it needs the Docker socket unless <see cref="GcpMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociGcpBuilder.WithBigQuery(BigQueryConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record BigQueryConfig : GcpMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "BIGQUERY";
}
