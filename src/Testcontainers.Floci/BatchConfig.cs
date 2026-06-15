using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AWS Batch emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithBatch(BatchConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder("floci/floci:1.5.25")
///     .WithBatch(new BatchConfig())
///     .Build();
/// </code>
/// In <c>immediate</c> runner mode (the default) jobs complete synchronously in-process without
/// spawning real containers, so no Docker socket is required. Other runner modes may spawn
/// sibling containers and therefore require Docker socket access.
/// </remarks>
[PublicAPI]
public sealed record BatchConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the job runner mode. Defaults to <c>"immediate"</c>, which completes jobs
    /// synchronously in-process without spawning any containers.
    /// </summary>
    public string RunnerMode { get; init; } = "immediate";

    /// <summary>
    /// Gets the Docker network that spawned job containers join, or <see langword="null" />
    /// to use the default bridge network. Only relevant when <see cref="RunnerMode" /> is not
    /// <c>"immediate"</c>.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <inheritdoc />
    protected override string ServiceKey => "BATCH";

    /// <inheritdoc />
    /// <remarks>
    /// Immediate mode completes jobs in-process — no container is spawned, so no Docker socket
    /// is needed. Any other runner mode may spawn sibling containers and requires socket access.
    /// </remarks>
    internal override bool RequiresDockerAccess =>
        !string.Equals(RunnerMode, "immediate", System.StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "RUNNER_MODE"] = RunnerMode;

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
