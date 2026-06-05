using System;
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
        return Merge(DockerResourceConfiguration, new FlociConfiguration(region: region))
            .WithEnvironment("FLOCI_DEFAULT_REGION", region);
    }

    /// <summary>
    /// Sets the default AWS account id the emulator advertises.
    /// </summary>
    /// <param name="accountId">The 12-digit AWS account id.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAccountId(string accountId)
    {
        return Merge(DockerResourceConfiguration, new FlociConfiguration(accountId: accountId))
            .WithEnvironment("FLOCI_DEFAULT_ACCOUNT_ID", accountId);
    }

    /// <summary>
    /// Sets the default availability zone the emulator advertises.
    /// </summary>
    /// <param name="availabilityZone">The availability zone, e.g. <c>eu-west-2a</c>.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAvailabilityZone(string availabilityZone)
    {
        return Merge(DockerResourceConfiguration, new FlociConfiguration(availabilityZone: availabilityZone))
            .WithEnvironment("FLOCI_DEFAULT_AVAILABILITY_ZONE", availabilityZone);
    }

    /// <summary>
    /// Configures Floci's CloudWatch Logs emulation.
    /// </summary>
    /// <param name="config">The CloudWatch Logs configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudWatchLogs(CloudWatchLogsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudWatch Metrics emulation.
    /// </summary>
    /// <param name="config">The CloudWatch Metrics configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudWatchMetrics(CloudWatchMetricsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's DynamoDB emulation.
    /// </summary>
    /// <param name="config">The DynamoDB configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithDynamoDb(DynamoDbConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ECS emulation. In real mode (the default) the Docker socket is mounted
    /// so Floci can spawn task containers; in mock mode no socket is needed.
    /// </summary>
    /// <param name="config">The ECS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEcs(EcsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ElastiCache emulation. Container-based: mounts the Docker socket and
    /// publishes the proxy port range so spawned Valkey/Memcached containers are reachable from
    /// the host.
    /// </summary>
    /// <param name="config">The ElastiCache configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithElastiCache(ElastiCacheConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EventBridge emulation.
    /// </summary>
    /// <param name="config">The EventBridge configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEventBridge(EventBridgeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's IAM emulation.
    /// </summary>
    /// <param name="config">The IAM configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithIam(IamConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Kinesis emulation.
    /// </summary>
    /// <param name="config">The Kinesis configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithKinesis(KinesisConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's KMS emulation.
    /// </summary>
    /// <param name="config">The KMS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithKms(KmsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Lambda emulation. Container-based: mounts the Docker socket so Floci can
    /// run each function's runtime container.
    /// </summary>
    /// <param name="config">The Lambda configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithLambda(LambdaConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's RDS emulation. Container-based: mounts the Docker socket and publishes
    /// the proxy port range so spawned database containers are reachable from the host.
    /// </summary>
    /// <param name="config">The RDS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithRds(RdsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's S3 emulation.
    /// </summary>
    /// <param name="config">The S3 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithS3(S3Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Secrets Manager emulation.
    /// </summary>
    /// <param name="config">The Secrets Manager configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSecretsManager(SecretsManagerConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's SSM emulation.
    /// </summary>
    /// <param name="config">The SSM configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSsm(SsmConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's SNS emulation.
    /// </summary>
    /// <param name="config">The SNS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSns(SnsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's SQS emulation.
    /// </summary>
    /// <param name="config">The SQS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSqs(SqsConfig config)
    {
        return WithServiceConfig(config);
    }

    /// <summary>
    /// Applies a per-service configuration by translating it to the corresponding
    /// <c>FLOCI_SERVICES_*</c> environment variables.
    /// </summary>
    /// <param name="config">The service configuration to apply.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    private FlociBuilder WithServiceConfig(FlociServiceConfig config)
    {
        var builder = this;
        foreach (var entry in config.BuildEnvironment())
        {
            builder = builder.WithEnvironment(entry.Key, entry.Value);
        }

        if (config.Enabled && config.RequiresDockerAccess)
        {
            builder = builder.WithDockerSocket();
        }

        if (config.Enabled)
        {
            foreach (var port in config.FixedHostPorts)
            {
                builder = builder.WithPortBinding(port, port);
            }
        }

        return builder;
    }

    /// <summary>
    /// Mounts the Docker socket into the Floci container so it can spawn sibling containers
    /// (required by container-based services such as RDS and Lambda). The source path respects
    /// the standard <c>TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE</c> environment variable, falling
    /// back to the conventional <c>/var/run/docker.sock</c> — so it works on native Linux Docker
    /// and VM-based setups (Colima, etc.) without any host-specific code.
    /// </summary>
    private FlociBuilder WithDockerSocket()
    {
        var socketPath = Environment.GetEnvironmentVariable("TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE");
        if (string.IsNullOrEmpty(socketPath))
        {
            socketPath = "/var/run/docker.sock";
        }

        return WithBindMount(socketPath, "/var/run/docker.sock");
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
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPath("/_floci/health").ForPort(FlociPort)));
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
