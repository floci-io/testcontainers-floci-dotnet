using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <inheritdoc cref="ContainerConfiguration" />
[PublicAPI]
public sealed class FlociGcpConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="dockerSocket">The forced Docker socket choice; see <see cref="DockerSocket" />.</param>
    public FlociGcpConfiguration(string? dockerSocket = null)
    {
        DockerSocket = dockerSocket;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="serviceConfigs">The per-service configs; see <see cref="ServiceConfigs" />.</param>
    public FlociGcpConfiguration(FlociServiceConfigs serviceConfigs)
    {
        ServiceConfigs = serviceConfigs;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociGcpConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociGcpConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociGcpConfiguration(FlociGcpConfiguration resourceConfiguration)
        : this(new FlociGcpConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public FlociGcpConfiguration(FlociGcpConfiguration oldValue, FlociGcpConfiguration newValue)
        : base(oldValue, newValue)
    {
        DockerSocket = BuildConfiguration.Combine(oldValue.DockerSocket, newValue.DockerSocket);
        ServiceConfigs = FlociServiceConfigs.Combine(oldValue.ServiceConfigs, newValue.ServiceConfigs);
    }

    /// <summary>
    /// Gets the forced Docker socket choice: <see langword="null" /> to detect it from the enabled
    /// services, an empty string to never mount it, or the host socket path to always mount.
    /// </summary>
    public string? DockerSocket { get; }

    /// <summary>
    /// Gets the per-service configs applied so far, one per service. Their environment variables
    /// and fixed host ports are written when the container is built.
    /// </summary>
    public FlociServiceConfigs ServiceConfigs { get; } = FlociServiceConfigs.Empty;
}
