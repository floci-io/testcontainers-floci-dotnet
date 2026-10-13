using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Database for PostgreSQL emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each server as a sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithPostgres(PostgresConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record PostgresConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresConfig" /> class.
    /// </summary>
    public PostgresConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "POSTGRES";
}
