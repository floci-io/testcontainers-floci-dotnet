using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's ElastiCache emulation.
/// </summary>
/// <remarks>
/// ElastiCache is a container-based service: Floci spawns a real Valkey or Memcached container
/// per cache cluster and fronts it with a proxy on a published port. This requires the Docker
/// socket (mounted automatically) and the proxy port range published 1:1 to the host.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithElastiCache(new ElastiCacheConfig { ProxyBasePort = 6390 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record ElastiCacheConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the base port of the ElastiCache proxy port range. Defaults to <c>6379</c>.
    /// </summary>
    public int ProxyBasePort { get; init; } = 6379;

    /// <summary>
    /// Gets the number of ports allocated to the ElastiCache proxy, starting at <see cref="ProxyBasePort" />.
    /// Defaults to <c>10</c>.
    /// </summary>
    public int ProxyPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets the default Docker image for Valkey/Redis instances. Defaults to <c>valkey/valkey:8</c>.
    /// </summary>
    public string Image { get; init; } = "valkey/valkey:8";

    /// <summary>
    /// Gets the default Docker image for Memcached instances. Defaults to <c>memcached:1.6</c>.
    /// </summary>
    public string MemcachedImage { get; init; } = "memcached:1.6";

    /// <summary>
    /// Gets the Docker network that spawned cache containers join, or <see langword="null" />
    /// to use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the highest port in the ElastiCache proxy port range.
    /// </summary>
    public int ProxyMaxPort => ProxyBasePort + ProxyPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "ELASTICACHE";

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
        env[prefix + "DEFAULT_IMAGE"] = Image;
        env[prefix + "DEFAULT_MEMCACHED_IMAGE"] = MemcachedImage;

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
