using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
[PublicAPI]
public sealed class FlociBuilder : ContainerBuilder<FlociBuilder, FlociContainer, FlociConfiguration>
{
    /// <summary>
    /// The Floci image. Pinning a digest is recommended for reproducible CI runs.
    /// </summary>
    public const string FlociImage = "floci/floci:latest";

    /// <summary>
    /// The single edge port that Floci exposes all AWS services on.
    /// </summary>
    public const ushort FlociPort = 4566;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociBuilder" /> class.
    /// </summary>
    public FlociBuilder()
        : this(new FlociConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private FlociBuilder(FlociConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override FlociConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Sets the default AWS region the emulator advertises.
    /// </summary>
    /// <param name="region">The AWS region, e.g. <c>eu-west-2</c>.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithRegion(string region)
    {
        // TODO(validate-against-floci): confirm the env var key Floci reads for the default region.
        return Merge(DockerResourceConfiguration, new FlociConfiguration(region: region))
            .WithEnvironment("AWS_DEFAULT_REGION", region);
    }

    /// <summary>
    /// Sets the default AWS account id the emulator advertises.
    /// </summary>
    /// <param name="accountId">The 12-digit AWS account id.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAccountId(string accountId)
    {
        // TODO(validate-against-floci): confirm the env var key Floci reads for the default account id.
        return Merge(DockerResourceConfiguration, new FlociConfiguration(accountId: accountId))
            .WithEnvironment("DEFAULT_ACCOUNT_ID", accountId);
    }

    /// <summary>
    /// Sets the default availability zone the emulator advertises.
    /// </summary>
    /// <param name="availabilityZone">The availability zone, e.g. <c>eu-west-2a</c>.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAvailabilityZone(string availabilityZone)
    {
        // TODO(validate-against-floci): confirm the env var key Floci reads for the default availability zone.
        return Merge(DockerResourceConfiguration, new FlociConfiguration(availabilityZone: availabilityZone))
            .WithEnvironment("DEFAULT_AVAILABILITY_ZONE", availabilityZone);
    }

    /// <inheritdoc />
    public override FlociContainer Build()
    {
        Validate();
        return new FlociContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override FlociBuilder Init()
    {
        return base.Init()
            .WithImage(FlociImage)
            .WithPortBinding(FlociPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(FlociPort));
    }

    /// <inheritdoc />
    protected override FlociBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociBuilder Merge(FlociConfiguration oldValue, FlociConfiguration newValue)
    {
        return new FlociBuilder(new FlociConfiguration(oldValue, newValue));
    }
}
