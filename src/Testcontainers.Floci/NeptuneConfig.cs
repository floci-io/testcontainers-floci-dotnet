using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Neptune emulation.
/// </summary>
/// <remarks>
/// Neptune is a container-based service: Floci spawns a Gremlin Server container per cluster via
/// the Docker socket and fronts it with a proxy inside the gateway. This requires the Docker socket
/// (mounted automatically) and the proxy port range published 1:1 to the host, so the Gremlin
/// endpoint is reachable.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithNeptune(new NeptuneConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record NeptuneConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the base port of the Neptune proxy port range. Defaults to <c>8182</c>.
    /// </summary>
    public int ProxyBasePort { get; init; } = 8182;

    /// <summary>
    /// Gets the number of ports allocated to the Neptune proxy, starting at
    /// <see cref="ProxyBasePort" />. Defaults to <c>101</c>.
    /// </summary>
    public int ProxyPortsCount { get; init; } = 101;

    /// <summary>
    /// Gets the default Docker image used for Neptune (Gremlin Server) instances. Defaults to
    /// <c>tinkerpop/gremlin-server:3.7.3</c>.
    /// </summary>
    public string DefaultImage { get; init; } = "tinkerpop/gremlin-server:3.7.3";

    /// <summary>
    /// Gets the Docker network that spawned Neptune containers join, or <see langword="null" />
    /// to use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the highest port in the Neptune proxy port range.
    /// </summary>
    public int ProxyMaxPort => ProxyBasePort + ProxyPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "NEPTUNE";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => true;

    /// <inheritdoc />
    /// <remarks>
    /// Neptune's proxy runs inside the Floci gateway (like RDS/ElastiCache), so the gateway
    /// publishes the proxy port range 1:1 to the host.
    /// </remarks>
    internal override IReadOnlyCollection<int> FixedHostPorts =>
        Enumerable.Range(ProxyBasePort, ProxyPortsCount).ToArray();

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "PROXY_BASE_PORT"] = ProxyBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "PROXY_MAX_PORT"] = ProxyMaxPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "DEFAULT_IMAGE"] = DefaultImage;

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
