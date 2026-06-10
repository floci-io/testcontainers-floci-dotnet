using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CodeBuild emulation.
/// </summary>
/// <remarks>
/// CodeBuild is a container-based service: Floci runs each build inside a Docker container spawned
/// via the Docker socket (mounted automatically).
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCodeBuild(new CodeBuildConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CodeBuildConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the Docker network that spawned build containers join, or <see langword="null" /> to
    /// use the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <inheritdoc />
    protected override string ServiceKey => "CODEBUILD";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => true;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
