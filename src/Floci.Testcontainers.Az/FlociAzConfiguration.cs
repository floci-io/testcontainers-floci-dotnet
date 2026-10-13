using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <inheritdoc cref="ContainerConfiguration" />
[PublicAPI]
public sealed class FlociAzConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzConfiguration" /> class.
    /// </summary>
    /// <param name="dockerSocket">The forced Docker socket choice; see <see cref="DockerSocket" />.</param>
    public FlociAzConfiguration(string? dockerSocket = null)
    {
        DockerSocket = dockerSocket;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociAzConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociAzConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociAzConfiguration(FlociAzConfiguration resourceConfiguration)
        : this(new FlociAzConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public FlociAzConfiguration(FlociAzConfiguration oldValue, FlociAzConfiguration newValue)
        : base(oldValue, newValue)
    {
        DockerSocket = BuildConfiguration.Combine(oldValue.DockerSocket, newValue.DockerSocket);
    }

    /// <summary>
    /// Gets the forced Docker socket choice: <see langword="null" /> to detect it from the enabled
    /// services, an empty string to never mount it, or the host socket path to always mount.
    /// </summary>
    public string? DockerSocket { get; }
}
