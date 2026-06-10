using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's OpenSearch emulation.
/// </summary>
/// <remarks>
/// OpenSearch is a container-based service: real mode (the default) spawns an OpenSearch container
/// per domain via the Docker socket and fronts it with a proxy on a published port; mock mode
/// simulates domains in-memory without starting any container. Real mode requires the Docker socket
/// (mounted automatically) and the proxy port range published 1:1 to the host.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithOpenSearch(new OpenSearchConfig { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record OpenSearchConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether OpenSearch domains are simulated in-memory without starting
    /// real Docker containers. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; }

    /// <summary>
    /// Gets the default Docker image used for OpenSearch instances, or <see langword="null" /> to
    /// use Floci's built-in default. Defaults to <see langword="null" />.
    /// </summary>
    public string? DefaultImage { get; init; }

    /// <summary>
    /// Gets the base port of the OpenSearch proxy port range. Defaults to <c>9400</c>.
    /// </summary>
    public int ProxyBasePort { get; init; } = 9400;

    /// <summary>
    /// Gets the number of ports allocated to the OpenSearch proxy, starting at
    /// <see cref="ProxyBasePort" />. Defaults to <c>10</c>.
    /// </summary>
    public int ProxyPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets the Docker network that spawned OpenSearch containers join, or <see langword="null" />
    /// to use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the highest port in the OpenSearch proxy port range.
    /// </summary>
    public int ProxyMaxPort => ProxyBasePort + ProxyPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "OPENSEARCH";

    /// <inheritdoc />
    /// <remarks>
    /// Mock mode simulates domains in-memory, so no Docker socket is needed. Real mode spawns
    /// OpenSearch containers and requires socket access.
    /// </remarks>
    internal override bool RequiresDockerAccess => !Mock;

    // Note: unlike RDS/ElastiCache/Neptune (whose proxy runs inside the Floci gateway, so the
    // gateway publishes the port range), Floci spawns the OpenSearch container with the proxy port
    // published directly on the host. The gateway must therefore NOT also bind that range, so we
    // deliberately do not override FixedHostPorts — doing so collides with Floci's own binding.

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";

        if (!string.IsNullOrEmpty(DefaultImage))
        {
            env[prefix + "DEFAULT_IMAGE"] = DefaultImage!;
        }

        env[prefix + "PROXY_BASE_PORT"] = ProxyBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "PROXY_MAX_PORT"] = ProxyMaxPort.ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
