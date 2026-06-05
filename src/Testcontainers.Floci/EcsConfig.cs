using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's ECS emulation.
/// </summary>
/// <remarks>
/// ECS supports two modes: real mode (the default) spawns actual Docker containers per task via
/// the Docker socket; mock mode returns tasks in RUNNING state immediately without starting any
/// containers. Real mode requires the Docker socket (mounted automatically).
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithEcs(new EcsConfig { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record EcsConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether ECS tasks go straight to RUNNING without starting real
    /// Docker containers. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; }

    /// <summary>
    /// Gets the default task memory size in megabytes. Defaults to <c>512</c>.
    /// </summary>
    public int MemoryMb { get; init; } = 512;

    /// <summary>
    /// Gets the default task CPU units. Defaults to <c>256</c>.
    /// </summary>
    public int CpuUnits { get; init; } = 256;

    /// <summary>
    /// Gets the Docker network that spawned task containers join, or <see langword="null" />
    /// to use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <inheritdoc />
    protected override string ServiceKey => "ECS";

    /// <inheritdoc />
    /// <remarks>
    /// Mock mode returns RUNNING without launching any container, so no Docker socket is needed.
    /// Real mode spawns actual task containers and requires socket access.
    /// </remarks>
    internal override bool RequiresDockerAccess => !Mock;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
        env[prefix + "DEFAULT_MEMORY_MB"] = MemoryMb.ToString(CultureInfo.InvariantCulture);
        env[prefix + "DEFAULT_CPU_UNITS"] = CpuUnits.ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
