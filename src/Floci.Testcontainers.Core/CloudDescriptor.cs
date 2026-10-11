using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Core;

/// <summary>
/// A service that spawns sibling containers and therefore needs the host Docker socket.
/// </summary>
/// <param name="Token">The SCREAMING_SNAKE name in <c>&lt;prefix&gt;SERVICES_&lt;Token&gt;_ENABLED</c>.</param>
/// <param name="Mockable">Whether <c>&lt;prefix&gt;SERVICES_&lt;Token&gt;_MOCK=true</c> means no containers are spawned.</param>
/// <param name="ModeSetting">
/// For services whose container use depends on a mode instead of a mock flag (e.g. Batch's
/// <c>RUNNER_MODE</c>): the setting that selects the mode. The socket is needed only when it is
/// set to something other than <paramref name="InProcessMode" />; an unset mode means the emulator
/// default, which is in-process.
/// </param>
/// <param name="InProcessMode">The mode value that spawns no containers.</param>
[PublicAPI]
public sealed record SocketService(string Token, bool Mockable = false, string? ModeSetting = null, string? InProcessMode = null);

/// <summary>
/// The facts that tell one Floci emulator apart from another, as data. A cloud package
/// (<c>Floci.Testcontainers.Aws</c> today; Azure, GCP and OCI later) is this descriptor plus its
/// own service configs and connection helpers.
/// </summary>
/// <param name="Name">Short cloud name, e.g. <c>aws</c>.</param>
/// <param name="Port">The emulator's single edge port.</param>
/// <param name="EnvPrefix">Prefix of every setting, e.g. <c>FLOCI_</c> or <c>FLOCI_AZ_</c>.</param>
/// <param name="HealthPath">HTTP path that answers 200 once the emulator is ready.</param>
/// <param name="ResetPath">HTTP path that wipes all emulator state on POST, or <see langword="null" />.</param>
/// <param name="SocketServices">Services that need the host Docker socket.</param>
[PublicAPI]
public sealed record CloudDescriptor(
    string Name,
    ushort Port,
    string EnvPrefix,
    string HealthPath,
    string? ResetPath,
    IReadOnlyList<SocketService> SocketServices)
{
    /// <summary>Gets the env var naming the Docker network sibling containers join.</summary>
    public string NetworkEnv => EnvPrefix + "SERVICES_DOCKER_NETWORK";

    /// <summary>Gets the env var that prefixes sibling container names, keeping parallel runs apart.</summary>
    public string ResourceNamespaceEnv => EnvPrefix + "DOCKER_RESOURCE_NAMESPACE";

    /// <summary>Gets the env var of one service setting, e.g. <c>ServiceEnv("SQS", "ENABLED")</c>.</summary>
    /// <param name="token">The service token.</param>
    /// <param name="setting">The setting name.</param>
    /// <returns>The fully qualified env var name.</returns>
    public string ServiceEnv(string token, string setting) => EnvPrefix + "SERVICES_" + token + "_" + setting;

    /// <summary>
    /// Returns whether any enabled socket service in <paramref name="env" /> spawns sibling containers.
    /// A missing <c>_ENABLED</c> key means enabled, which is Floci's default.
    /// </summary>
    /// <param name="env">The container environment, as it will be when the container starts.</param>
    /// <returns><see langword="true" /> when the host Docker socket is needed.</returns>
    public bool DockerSocketRequired(IReadOnlyDictionary<string, string> env)
    {
        return SocketServices.Any(svc => NeedsSocket(svc, env));
    }

    /// <summary>Returns a unique resource namespace such as <c>tc-1a2b3c4d</c>.</summary>
    /// <returns>The namespace.</returns>
    public static string NewNamespace() => "tc-" + Guid.NewGuid().ToString("N").Substring(0, 8);

    private bool NeedsSocket(SocketService svc, IReadOnlyDictionary<string, string> env)
    {
        if (env.TryGetValue(ServiceEnv(svc.Token, "ENABLED"), out var enabled)
            && !string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (svc.Mockable
            && env.TryGetValue(ServiceEnv(svc.Token, "MOCK"), out var mock)
            && string.Equals(mock, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (svc.ModeSetting is not null)
        {
            return env.TryGetValue(ServiceEnv(svc.Token, svc.ModeSetting), out var mode)
                && !string.Equals(mode, svc.InProcessMode, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }
}
