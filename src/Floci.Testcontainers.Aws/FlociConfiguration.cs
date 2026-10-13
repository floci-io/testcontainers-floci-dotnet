using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Aws;

/// <inheritdoc cref="ContainerConfiguration" />
[PublicAPI]
public sealed class FlociConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="region">The default AWS region the emulator advertises.</param>
    /// <param name="accountId">The default AWS account id the emulator advertises.</param>
    /// <param name="availabilityZone">The default availability zone the emulator advertises.</param>
    /// <param name="dockerSocket">The forced Docker socket choice; see <see cref="DockerSocket" />.</param>
    public FlociConfiguration(
        string? region = null,
        string? accountId = null,
        string? availabilityZone = null,
        string? dockerSocket = null)
    {
        DockerSocket = dockerSocket;
        Region = region;
        AccountId = accountId;
        AvailabilityZone = availabilityZone;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="serviceConfigs">The per-service configs; see <see cref="ServiceConfigs" />.</param>
    public FlociConfiguration(FlociServiceConfigs serviceConfigs)
    {
        ServiceConfigs = serviceConfigs;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public FlociConfiguration(FlociConfiguration resourceConfiguration)
        : this(new FlociConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public FlociConfiguration(FlociConfiguration oldValue, FlociConfiguration newValue)
        : base(oldValue, newValue)
    {
        Region = BuildConfiguration.Combine(oldValue.Region, newValue.Region);
        AccountId = BuildConfiguration.Combine(oldValue.AccountId, newValue.AccountId);
        AvailabilityZone = BuildConfiguration.Combine(oldValue.AvailabilityZone, newValue.AvailabilityZone);
        DockerSocket = BuildConfiguration.Combine(oldValue.DockerSocket, newValue.DockerSocket);
        ServiceConfigs = FlociServiceConfigs.Combine(oldValue.ServiceConfigs, newValue.ServiceConfigs);
    }

    /// <summary>
    /// Gets the default AWS region the emulator advertises.
    /// </summary>
    public string? Region { get; }

    /// <summary>
    /// Gets the default AWS account id the emulator advertises.
    /// </summary>
    public string? AccountId { get; }

    /// <summary>
    /// Gets the default availability zone the emulator advertises.
    /// </summary>
    public string? AvailabilityZone { get; }

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
