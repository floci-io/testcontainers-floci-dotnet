using System;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
[PublicAPI]
public sealed class FlociBuilder : ContainerBuilder<FlociBuilder, FlociContainer, FlociConfiguration>
{
    private const string DefaultImage = "floci/floci:1.5.22";

    /// <summary>
    /// The default Floci image.
    /// </summary>
    [Obsolete("Pass the image to a constructor instead, e.g. new FlociBuilder(\"floci/floci:1.5.22\"). A baked-in default image will be removed in a future version.")]
    public const string FlociImage = DefaultImage;

    /// <summary>
    /// The single edge port that Floci exposes all AWS services on.
    /// </summary>
    public const ushort FlociPort = 4566;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociBuilder" /> class using the default image.
    /// </summary>
    [Obsolete("Use a constructor that takes the image, e.g. new FlociBuilder(\"floci/floci:1.5.22\"). The parameterless constructor will be removed in a future version.")]
    public FlociBuilder()
        : this(DefaultImage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full Docker image name, including repository and tag (e.g. <c>floci/floci:1.5.22</c>).
    /// This also lets you point at a private registry mirror.
    /// </param>
    public FlociBuilder(string image)
        : this(new DockerImage(image))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociBuilder" /> class.
    /// </summary>
    /// <param name="image">The Docker image to use for the container.</param>
    public FlociBuilder(IImage image)
        : this(new FlociConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
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
    /// Configures Floci's ACM (Certificate Manager) emulation.
    /// </summary>
    /// <param name="config">The ACM configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAcm(AcmConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's API Gateway (v1) emulation.
    /// </summary>
    /// <param name="config">The API Gateway configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithApiGateway(ApiGatewayConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's API Gateway V2 emulation.
    /// </summary>
    /// <param name="config">The API Gateway V2 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithApiGatewayV2(ApiGatewayV2Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AppConfig emulation.
    /// </summary>
    /// <param name="config">The AppConfig configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAppConfig(AppConfigConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AppConfig Data emulation.
    /// </summary>
    /// <param name="config">The AppConfig Data configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAppConfigData(AppConfigDataConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AppSync emulation.
    /// </summary>
    /// <param name="config">The AppSync configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAppSync(AppSyncConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EC2 Auto Scaling emulation. Auto Scaling is a standalone service in Floci
    /// (independent of EC2) and is enabled by default.
    /// </summary>
    /// <param name="config">The Auto Scaling configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAutoScaling(AutoScalingConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudFormation emulation.
    /// </summary>
    /// <param name="config">The CloudFormation configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudFormation(CloudFormationConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudTrail emulation.
    /// </summary>
    /// <param name="config">The CloudTrail configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudTrail(CloudTrailConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Map (service discovery) emulation.
    /// </summary>
    /// <param name="config">The Cloud Map configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudMap(CloudMapConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Athena emulation.
    /// </summary>
    /// <param name="config">The Athena configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAthena(AthenaConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudWatch Logs emulation.
    /// </summary>
    /// <param name="config">The CloudWatch Logs configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudWatchLogs(CloudWatchLogsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AWS Backup emulation.
    /// </summary>
    /// <param name="config">The Backup configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithBackup(BackupConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AWS Batch emulation.
    /// </summary>
    /// <param name="config">The Batch configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithBatch(BatchConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudWatch Metrics emulation.
    /// </summary>
    /// <param name="config">The CloudWatch Metrics configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudWatchMetrics(CloudWatchMetricsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's BCM Data Exports emulation.
    /// </summary>
    /// <param name="config">The BCM Data Exports configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithBcmDataExports(BcmDataExportsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cognito emulation.
    /// </summary>
    /// <param name="config">The Cognito configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCognito(CognitoConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Bedrock Runtime emulation.
    /// </summary>
    /// <param name="config">The Bedrock Runtime configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithBedrockRuntime(BedrockRuntimeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's DynamoDB emulation.
    /// </summary>
    /// <param name="config">The DynamoDB configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithDynamoDb(DynamoDbConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CloudFront emulation.
    /// </summary>
    /// <param name="config">The CloudFront configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCloudFront(CloudFrontConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EC2 emulation. In real mode (the default) the Docker socket is mounted
    /// so Floci can back instances with containers; in mock mode no socket is needed.
    /// </summary>
    /// <param name="config">The EC2 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEc2(Ec2Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CodeBuild emulation. Container-based: mounts the Docker socket so Floci
    /// can run each build inside a spawned container.
    /// </summary>
    /// <param name="config">The CodeBuild configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCodeBuild(CodeBuildConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's CodeDeploy emulation.
    /// </summary>
    /// <param name="config">The CodeDeploy configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCodeDeploy(CodeDeployConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ECR emulation. Container-based: mounts the Docker socket and publishes
    /// the registry port range so the backing registry container is reachable from the host.
    /// </summary>
    /// <param name="config">The ECR configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEcr(EcrConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's AWS Config emulation.
    /// </summary>
    /// <param name="config">The AWS Config configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithAwsConfig(AwsConfigConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ECS emulation. In real mode (the default) the Docker socket is mounted
    /// so Floci can spawn task containers; in mock mode no socket is needed.
    /// </summary>
    /// <param name="config">The ECS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEcs(EcsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EKS emulation. In real mode (the default) the Docker socket is mounted so
    /// Floci can spawn a k3s container per cluster, publishing its API server on a host port; in
    /// mock mode clusters return ACTIVE without starting any container.
    /// </summary>
    /// <param name="config">The EKS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEks(EksConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EMR (Elastic MapReduce) emulation.
    /// </summary>
    /// <param name="config">The EMR configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEmr(EmrConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cost Explorer emulation.
    /// </summary>
    /// <param name="config">The Cost Explorer configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCostExplorer(CostExplorerConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ElastiCache emulation. Container-based: mounts the Docker socket and
    /// publishes the proxy port range so spawned Valkey/Memcached containers are reachable from
    /// the host.
    /// </summary>
    /// <param name="config">The ElastiCache configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithElastiCache(ElastiCacheConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cost and Usage Reports (CUR) emulation.
    /// </summary>
    /// <param name="config">The CUR configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithCur(CurConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EventBridge emulation.
    /// </summary>
    /// <param name="config">The EventBridge configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithEventBridge(EventBridgeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's ELBv2 (Elastic Load Balancing v2) emulation.
    /// </summary>
    /// <param name="config">The ELBv2 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithElbV2(ElbV2Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Firehose emulation.
    /// </summary>
    /// <param name="config">The Firehose configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithFirehose(FirehoseConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EventBridge Pipes emulation.
    /// </summary>
    /// <param name="config">The Pipes configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithPipes(PipesConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Glue emulation.
    /// </summary>
    /// <param name="config">The Glue configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithGlue(GlueConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Pricing emulation.
    /// </summary>
    /// <param name="config">The Pricing configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithPricing(PricingConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's IAM emulation.
    /// </summary>
    /// <param name="config">The IAM configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithIam(IamConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Route 53 emulation.
    /// </summary>
    /// <param name="config">The Route 53 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithRoute53(Route53Config config) => WithServiceConfig(config);

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
    /// Configures Floci's Textract emulation.
    /// </summary>
    /// <param name="config">The Textract configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithTextract(TextractConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Transcribe emulation.
    /// </summary>
    /// <param name="config">The Transcribe configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithTranscribe(TranscribeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Transfer Family emulation.
    /// </summary>
    /// <param name="config">The Transfer Family configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithTransferFamily(TransferFamilyConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Lambda emulation. Container-based: mounts the Docker socket so Floci can
    /// run each function's runtime container.
    /// </summary>
    /// <param name="config">The Lambda configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithLambda(LambdaConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's MSK (Managed Streaming for Apache Kafka) emulation. Container-based:
    /// mounts the Docker socket so Floci can spawn a Redpanda broker container per cluster (unless
    /// mock mode is enabled).
    /// </summary>
    /// <param name="config">The MSK configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithMsk(MskConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Neptune emulation. Container-based: mounts the Docker socket and publishes
    /// the proxy port range so the spawned Gremlin Server is reachable from the host.
    /// </summary>
    /// <param name="config">The Neptune configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithNeptune(NeptuneConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's OpenSearch emulation. Container-based: mounts the Docker socket and
    /// publishes the proxy port range so spawned OpenSearch containers are reachable from the host
    /// (unless mock mode is enabled).
    /// </summary>
    /// <param name="config">The OpenSearch configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithOpenSearch(OpenSearchConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's RDS emulation. Container-based: mounts the Docker socket and publishes
    /// the proxy port range so spawned database containers are reachable from the host.
    /// </summary>
    /// <param name="config">The RDS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithRds(RdsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's RDS Data API emulation.
    /// </summary>
    /// <param name="config">The RDS Data API configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithRdsData(RdsDataConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Resource Groups Tagging emulation.
    /// </summary>
    /// <param name="config">The Resource Groups Tagging configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithResourceGroupsTagging(ResourceGroupsTaggingConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's S3 emulation.
    /// </summary>
    /// <param name="config">The S3 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithS3(S3Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's EventBridge Scheduler emulation.
    /// </summary>
    /// <param name="config">The Scheduler configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithScheduler(SchedulerConfig config) => WithServiceConfig(config);

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
    /// Configures Floci's SES emulation.
    /// </summary>
    /// <param name="config">The SES configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSes(SesConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's SES v2 emulation.
    /// </summary>
    /// <param name="config">The SES v2 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSesV2(SesV2Config config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's SNS emulation.
    /// </summary>
    /// <param name="config">The SNS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithSns(SnsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Step Functions emulation.
    /// </summary>
    /// <param name="config">The Step Functions configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithStepFunctions(StepFunctionsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's WAF v2 emulation.
    /// </summary>
    /// <param name="config">The WAF v2 configuration.</param>
    /// <returns>A configured instance of <see cref="FlociBuilder" />.</returns>
    public FlociBuilder WithWafV2(WafV2Config config) => WithServiceConfig(config);

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
