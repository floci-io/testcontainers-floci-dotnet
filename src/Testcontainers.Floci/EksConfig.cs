using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EKS (Elastic Kubernetes Service) emulation.
/// </summary>
/// <remarks>
/// EKS is a container-based service: real mode (the default) spawns a real Kubernetes container
/// (k3s) per cluster via the Docker socket and publishes its API server directly on a host port
/// from the API-server range; mock mode returns clusters as <c>ACTIVE</c> without starting any
/// container. Real mode requires the Docker socket (mounted automatically).
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithEks(new EksConfig { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record EksConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether clusters go straight to <c>ACTIVE</c> without starting real
    /// Docker containers. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; }

    /// <summary>
    /// Gets the Kubernetes provider used for EKS clusters. Defaults to <c>k3s</c>.
    /// </summary>
    public string Provider { get; init; } = "k3s";

    /// <summary>
    /// Gets the default Docker image used for EKS (k3s) instances. Defaults to
    /// <c>rancher/k3s:latest</c>.
    /// </summary>
    public string DefaultImage { get; init; } = "rancher/k3s:latest";

    /// <summary>
    /// Gets the base port of the EKS API server port range. Defaults to <c>6500</c>.
    /// </summary>
    public int ApiServerBasePort { get; init; } = 6500;

    /// <summary>
    /// Gets the number of ports allocated to the EKS API server, starting at
    /// <see cref="ApiServerBasePort" />. Defaults to <c>10</c>.
    /// </summary>
    public int ApiServerPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets the Docker network that spawned EKS containers join, or <see langword="null" /> to use
    /// the default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the endpoint mode used in <c>describe-cluster</c> responses. <c>host</c> (the default)
    /// returns <c>https://localhost:&lt;hostPort&gt;</c>, reachable from the host; <c>network</c>
    /// returns the container DNS name, reachable from other containers on the Docker network.
    /// </summary>
    public string EndpointMode { get; init; } = "host";

    /// <summary>
    /// Gets a value indicating whether a token-authentication webhook is wired into k3s so the
    /// bearer token produced by <c>aws eks get-token</c> is validated by Floci and mapped to
    /// cluster-admin. Defaults to <see langword="true" />.
    /// </summary>
    public bool IamAuthWebhook { get; init; } = true;

    /// <summary>
    /// Gets the highest port in the EKS API server port range.
    /// </summary>
    public int ApiServerMaxPort => ApiServerBasePort + ApiServerPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "EKS";

    /// <inheritdoc />
    /// <remarks>
    /// Mock mode returns clusters as ACTIVE without launching any container, so no Docker socket is
    /// needed. Real mode spawns k3s containers and requires socket access.
    /// </remarks>
    internal override bool RequiresDockerAccess => !Mock;

    // Note: like OpenSearch (and unlike RDS/Neptune), Floci publishes the spawned k3s container's
    // API server directly on the host port — the gateway must NOT also bind the API-server range,
    // so FixedHostPorts is deliberately not overridden (doing so collides with Floci's binding).

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
        env[prefix + "PROVIDER"] = Provider;
        env[prefix + "DEFAULT_IMAGE"] = DefaultImage;
        env[prefix + "API_SERVER_BASE_PORT"] = ApiServerBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "API_SERVER_MAX_PORT"] = ApiServerMaxPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "ENDPOINT_MODE"] = EndpointMode;
        env[prefix + "IAM_AUTH_WEBHOOK"] = IamAuthWebhook ? "true" : "false";

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
