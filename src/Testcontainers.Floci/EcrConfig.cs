using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's ECR emulation.
/// </summary>
/// <remarks>
/// ECR is container-based: Floci runs a real registry container (default <c>registry:2</c>) to
/// back image push/pull, fronted on a published port range. Requires the Docker socket (mounted
/// automatically). Control-plane operations (CreateRepository, etc.) go through the gateway.
/// </remarks>
[PublicAPI]
public sealed record EcrConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the Docker image used for the backing registry. Defaults to <c>registry:2</c>.
    /// </summary>
    public string RegistryImage { get; init; } = "registry:2";

    /// <summary>
    /// Gets the name of the backing registry container. Defaults to <c>floci-ecr-registry</c>.
    /// </summary>
    public string RegistryContainerName { get; init; } = "floci-ecr-registry";

    /// <summary>
    /// Gets the base port of the registry port range. Defaults to <c>5100</c>.
    /// </summary>
    public int RegistryBasePort { get; init; } = 5100;

    /// <summary>
    /// Gets the number of ports allocated to the registry, starting at <see cref="RegistryBasePort" />.
    /// Defaults to <c>10</c>.
    /// </summary>
    public int RegistryPortsCount { get; init; } = 10;

    /// <summary>
    /// Gets a value indicating whether TLS is enabled for the registry. Defaults to <see langword="false" />.
    /// </summary>
    public bool TlsEnabled { get; init; }

    /// <summary>
    /// Gets the URI style for <c>repositoryUri</c> responses: <c>hostname</c> or <c>path</c>.
    /// Defaults to <c>hostname</c>.
    /// </summary>
    public string UriStyle { get; init; } = "hostname";

    /// <summary>
    /// Gets the Docker network the registry container joins, or <see langword="null" /> for the
    /// default bridge network.
    /// </summary>
    public string? DockerNetwork { get; init; }

    /// <summary>
    /// Gets the highest port in the registry port range.
    /// </summary>
    public int RegistryMaxPort => RegistryBasePort + RegistryPortsCount - 1;

    /// <inheritdoc />
    protected override string ServiceKey => "ECR";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => true;

    /// <inheritdoc />
    internal override IReadOnlyCollection<int> FixedHostPorts =>
        Enumerable.Range(RegistryBasePort, RegistryPortsCount).ToArray();

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "REGISTRY_IMAGE"] = RegistryImage;
        env[prefix + "REGISTRY_CONTAINER_NAME"] = RegistryContainerName;
        env[prefix + "REGISTRY_BASE_PORT"] = RegistryBasePort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "REGISTRY_MAX_PORT"] = RegistryMaxPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "TLS_ENABLED"] = TlsEnabled ? "true" : "false";
        env[prefix + "URI_STYLE"] = UriStyle;
        env[prefix + "KEEP_RUNNING_ON_SHUTDOWN"] = "false";

        if (!string.IsNullOrEmpty(DockerNetwork))
        {
            env[prefix + "DOCKER_NETWORK"] = DockerNetwork!;
        }
    }
}
