using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's RDS emulation.
/// </summary>
/// <remarks>
/// RDS is a container-based service: Floci spawns a real database container (PostgreSQL, MySQL,
/// or MariaDB) per instance and fronts it with a proxy on a published port. This requires the
/// Docker socket (mounted automatically) and the proxy port range published 1:1 to the host.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithRds(new RdsConfig { PostgresImage = "postgres:16-alpine" })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record RdsConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the base port of the RDS proxy port range. Defaults to <c>7000</c>.
    /// </summary>
    public int ProxyBasePort { get; init; } = 7000;

    /// <summary>
    /// Gets the number of ports allocated to the RDS proxy, starting at <see cref="ProxyBasePort" />.
    /// Defaults to <c>10</c>.
    /// </summary>
    public int ProxyPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets the default Docker image for PostgreSQL instances. Defaults to <c>postgres:16-alpine</c>.
    /// </summary>
    public string PostgresImage { get; init; } = "postgres:16-alpine";

    /// <summary>
    /// Gets the default Docker image for MySQL instances. Defaults to <c>mysql:8.0</c>.
    /// </summary>
    public string MysqlImage { get; init; } = "mysql:8.0";

    /// <summary>
    /// Gets the default Docker image for MariaDB instances. Defaults to <c>mariadb:11</c>.
    /// </summary>
    public string MariadbImage { get; init; } = "mariadb:11";

    /// <summary>
    /// Gets the Docker network that spawned database containers join, or <see langword="null" />
    /// to use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the highest port in the RDS proxy port range.
    /// </summary>
    public int ProxyMaxPort => ProxyBasePort + ProxyPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "RDS";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => true;

    /// <inheritdoc />
    internal override IReadOnlyCollection<int> FixedHostPorts =>
        Enumerable.Range(ProxyBasePort, ProxyPortsCount).ToArray();

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "PROXY_BASE_PORT"] = ProxyBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "PROXY_MAX_PORT"] = ProxyMaxPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "DEFAULT_POSTGRES_IMAGE"] = PostgresImage;
        env[prefix + "DEFAULT_MYSQL_IMAGE"] = MysqlImage;
        env[prefix + "DEFAULT_MARIADB_IMAGE"] = MariadbImage;

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
