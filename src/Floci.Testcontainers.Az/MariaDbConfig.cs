using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Database for MariaDB emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each server as a sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithMariaDb(MariaDbConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record MariaDbConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MariaDbConfig" /> class.
    /// </summary>
    public MariaDbConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "MARIA_DB";
}
