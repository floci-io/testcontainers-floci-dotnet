using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Lambda emulation.
/// </summary>
/// <remarks>
/// Lambda is a container-based service: Floci runs each function's real AWS Lambda runtime
/// container (pulled from public ECR on first invoke) via the Docker daemon, so the Docker
/// socket is mounted automatically. Invocations are proxied through the Floci gateway, so the
/// Runtime API ports do not need publishing unless <see cref="ExposeRuntimePorts" /> is set.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithLambda(new LambdaConfig { MemoryMb = 256, TimeoutSeconds = 10 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record LambdaConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether function containers are removed after each invocation.
    /// Defaults to <see langword="false" /> (a warm container is reused).
    /// </summary>
    public bool Ephemeral { get; init; }

    /// <summary>
    /// Gets the default function memory size in megabytes. Defaults to <c>128</c>.
    /// </summary>
    public int MemoryMb { get; init; } = 128;

    /// <summary>
    /// Gets the default function timeout in seconds. Defaults to <c>3</c>.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 3;

    /// <summary>
    /// Gets the base port of the Lambda Runtime API port range. Defaults to <c>9200</c>.
    /// </summary>
    public int RuntimeApiBasePort { get; init; } = 9200;

    /// <summary>
    /// Gets the number of ports allocated to the Lambda Runtime API. Defaults to <c>10</c>.
    /// </summary>
    public int RuntimeApiPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets a value indicating whether the Runtime API ports are published to the host. Defaults
    /// to <see langword="false" /> — invocations go through the gateway, so this is rarely needed.
    /// </summary>
    public bool ExposeRuntimePorts { get; init; }

    /// <summary>
    /// Gets the poll interval in milliseconds for function container status checks. Defaults to <c>1000</c>.
    /// </summary>
    public int PollIntervalMs { get; init; } = 1000;

    /// <summary>
    /// Gets the idle timeout in seconds after which unused function containers are cleaned up.
    /// Defaults to <c>300</c>.
    /// </summary>
    public int ContainerIdleTimeoutSeconds { get; init; } = 300;

    /// <summary>
    /// Gets the per-region concurrent executions ceiling. Defaults to <c>1000</c>.
    /// </summary>
    public int RegionConcurrencyLimit { get; init; } = 1000;

    /// <summary>
    /// Gets the minimum unreserved concurrency that must remain after a reservation. Defaults to <c>100</c>.
    /// </summary>
    public int UnreservedConcurrencyMin { get; init; } = 100;

    /// <summary>
    /// Gets a value indicating whether hot-reload development mode is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool HotReloadEnabled { get; init; }

    /// <summary>
    /// Gets the optional allow-list of absolute path prefixes for hot-reload, or <see langword="null" />
    /// for no restriction.
    /// </summary>
    public IReadOnlyList<string>? HotReloadAllowedPaths { get; init; }

    /// <summary>
    /// Gets the Docker network that spawned function containers join, or <see langword="null" />
    /// for the default network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets a host path that Floci bind-mounts (read-only) into each function container at
    /// <c>/opt/aws-config</c>, or <see langword="null" /> to inject dummy credentials as usual.
    /// When set, Floci points <c>AWS_SHARED_CREDENTIALS_FILE</c> and <c>AWS_CONFIG_FILE</c> at the
    /// mounted files instead. Note: the path is resolved by the host Docker daemon, so on VM-based
    /// setups (Colima) it must be a path the VM can see, not a macOS host path.
    /// </summary>
    public string? AwsConfigPath { get; init; }

    /// <summary>
    /// Gets the highest port in the Lambda Runtime API port range.
    /// </summary>
    public int RuntimeApiMaxPort => RuntimeApiBasePort + RuntimeApiPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "LAMBDA";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => true;

    /// <inheritdoc />
    internal override IReadOnlyCollection<int> FixedHostPorts =>
        ExposeRuntimePorts
            ? Enumerable.Range(RuntimeApiBasePort, RuntimeApiPortsCount).ToArray()
            : System.Array.Empty<int>();

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "EPHEMERAL"] = Ephemeral ? "true" : "false";
        env[prefix + "DEFAULT_MEMORY_MB"] = MemoryMb.ToString(CultureInfo.InvariantCulture);
        env[prefix + "DEFAULT_TIMEOUT_SECONDS"] = TimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        env[prefix + "RUNTIME_API_BASE_PORT"] = RuntimeApiBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "RUNTIME_API_MAX_PORT"] = RuntimeApiMaxPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "POLL_INTERVAL_MS"] = PollIntervalMs.ToString(CultureInfo.InvariantCulture);
        env[prefix + "CONTAINER_IDLE_TIMEOUT_SECONDS"] = ContainerIdleTimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        env[prefix + "REGION_CONCURRENCY_LIMIT"] = RegionConcurrencyLimit.ToString(CultureInfo.InvariantCulture);
        env[prefix + "UNRESERVED_CONCURRENCY_MIN"] = UnreservedConcurrencyMin.ToString(CultureInfo.InvariantCulture);
        env[prefix + "HOT_RELOAD_ENABLED"] = HotReloadEnabled ? "true" : "false";

        if (HotReloadAllowedPaths is { Count: > 0 })
        {
            env[prefix + "HOT_RELOAD_ALLOWED_PATHS"] = string.Join(",", HotReloadAllowedPaths);
        }

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }

        if (!string.IsNullOrWhiteSpace(AwsConfigPath))
        {
            env[prefix + "AWS_CONFIG_PATH"] = AwsConfigPath!;
        }
    }
}
