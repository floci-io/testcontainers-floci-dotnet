using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud SQL emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each instance as a PostgreSQL or MySQL sibling container, so it needs the Docker socket unless <see cref="GcpMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociGcpBuilder.WithCloudSql(CloudSqlConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record CloudSqlConfig : GcpMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDSQL";
}
