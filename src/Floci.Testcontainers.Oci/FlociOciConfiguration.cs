using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <inheritdoc cref="ContainerConfiguration" />
[PublicAPI]
public sealed class FlociOciConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="dockerSocket">The forced Docker socket choice; see <see cref="DockerSocket" />.</param>
    /// <param name="privateKeyPem">The private key to hand out; see <see cref="PrivateKeyPem" />.</param>
    public FlociOciConfiguration(string? dockerSocket = null, string? privateKeyPem = null)
    {
        DockerSocket = dockerSocket;
        PrivateKeyPem = privateKeyPem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="serviceConfigs">The per-service configs; see <see cref="ServiceConfigs" />.</param>
    public FlociOciConfiguration(FlociServiceConfigs serviceConfigs)
    {
        ServiceConfigs = serviceConfigs;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociOciConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociOciConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociOciConfiguration(FlociOciConfiguration resourceConfiguration)
        : this(new FlociOciConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public FlociOciConfiguration(FlociOciConfiguration oldValue, FlociOciConfiguration newValue)
        : base(oldValue, newValue)
    {
        DockerSocket = BuildConfiguration.Combine(oldValue.DockerSocket, newValue.DockerSocket);
        ServiceConfigs = FlociServiceConfigs.Combine(oldValue.ServiceConfigs, newValue.ServiceConfigs);
        PrivateKeyPem = BuildConfiguration.Combine(oldValue.PrivateKeyPem, newValue.PrivateKeyPem);
    }

    /// <summary>
    /// Gets the forced Docker socket choice: <see langword="null" /> to detect it from the enabled
    /// services, an empty string to never mount it, or the host socket path to always mount.
    /// </summary>
    public string? DockerSocket { get; }

    /// <summary>
    /// Gets the PEM-encoded private key the container hands out for signing requests, or
    /// <see langword="null" /> to generate a throwaway one.
    /// </summary>
    public string? PrivateKeyPem { get; }

    /// <summary>
    /// Gets the per-service configs applied so far, one per service. Their environment variables
    /// and fixed host ports are written when the container is built.
    /// </summary>
    public FlociServiceConfigs ServiceConfigs { get; } = FlociServiceConfigs.Empty;
}
