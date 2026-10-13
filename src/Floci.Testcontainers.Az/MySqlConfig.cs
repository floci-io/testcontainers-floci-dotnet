using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Database for MySQL emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each server as a sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithMySql(MySqlConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record MySqlConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlConfig" /> class.
    /// </summary>
    public MySqlConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "MYSQL";
}
