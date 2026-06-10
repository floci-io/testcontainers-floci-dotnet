using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's MSK (Managed Streaming for Apache Kafka) emulation.
/// </summary>
/// <remarks>
/// MSK supports two modes: real mode (the default) spawns a Redpanda broker container per cluster
/// via the Docker socket; mock mode simulates clusters in-memory without starting any container.
/// Real mode requires the Docker socket (mounted automatically).
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithMsk(new MskConfig { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record MskConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether MSK clusters are simulated in-memory without starting real
    /// Docker containers. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; }

    /// <summary>
    /// Gets the default Docker image used for MSK (Redpanda) broker instances. Defaults to
    /// <c>redpandadata/redpanda:latest</c>.
    /// </summary>
    public string DefaultImage { get; init; } = "redpandadata/redpanda:latest";

    /// <inheritdoc />
    protected override string ServiceKey => "MSK";

    /// <inheritdoc />
    /// <remarks>
    /// Mock mode simulates clusters in-memory, so no Docker socket is needed. Real mode spawns a
    /// Redpanda broker container and requires socket access.
    /// </remarks>
    internal override bool RequiresDockerAccess => !Mock;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
        env[prefix + "DEFAULT_IMAGE"] = DefaultImage;
    }
}
