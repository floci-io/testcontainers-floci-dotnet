using System.Collections.Generic;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
/// <remarks>
/// <code>
/// await using var floci = new FlociGcpBuilder("floci/floci-gcp:latest").Build();
/// await floci.StartAsync();
/// var storage = new StorageClientBuilder { BaseUri = floci.GetEndpoint() + "/storage/v1/", UnauthenticatedAccess = true }.Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed class FlociGcpBuilder : ContainerBuilder<FlociGcpBuilder, FlociGcpContainer, FlociGcpConfiguration>
{
    /// <summary>
    /// The single edge port that Floci GCP exposes all Google Cloud services on, over HTTP and gRPC.
    /// </summary>
    public const ushort FlociGcpPort = 4588;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full Docker image name, including repository and tag (e.g. <c>floci/floci-gcp:x.y.z</c>).
    /// This also lets you point at a private registry mirror.
    /// </param>
    public FlociGcpBuilder(string image)
        : this(new DockerImage(image))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpBuilder" /> class.
    /// </summary>
    /// <param name="image">The Docker image to use for the container.</param>
    public FlociGcpBuilder(IImage image)
        : this(new FlociGcpConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private FlociGcpBuilder(FlociGcpConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override FlociGcpConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Configures Floci's BigQuery emulation.
    /// </summary>
    /// <param name="config">The BigQuery configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithBigQuery(BigQueryConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Functions emulation.
    /// </summary>
    /// <param name="config">The Cloud Functions configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithCloudFunctions(CloudFunctionsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Run emulation.
    /// </summary>
    /// <param name="config">The Cloud Run configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithCloudRun(CloudRunConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud SQL emulation.
    /// </summary>
    /// <param name="config">The Cloud SQL configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithCloudSql(CloudSqlConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Tasks emulation.
    /// </summary>
    /// <param name="config">The Cloud Tasks configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithCloudTasks(CloudTasksConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Compute Engine emulation.
    /// </summary>
    /// <param name="config">The Compute Engine configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithCompute(ComputeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Datastore emulation.
    /// </summary>
    /// <param name="config">The Datastore configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithDatastore(DatastoreConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Eventarc emulation.
    /// </summary>
    /// <param name="config">The Eventarc configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithEventarc(EventarcConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Firebase Authentication (Identity Platform) emulation.
    /// </summary>
    /// <param name="config">The Firebase Authentication (Identity Platform) configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithFirebaseAuth(FirebaseAuthConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Firestore emulation.
    /// </summary>
    /// <param name="config">The Firestore configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithFirestore(FirestoreConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Storage emulation.
    /// </summary>
    /// <param name="config">The Cloud Storage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithGcs(GcsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Google Kubernetes Engine (GKE) emulation.
    /// </summary>
    /// <param name="config">The Google Kubernetes Engine (GKE) configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithGke(GkeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's IAM emulation.
    /// </summary>
    /// <param name="config">The IAM configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithIam(GcpIamConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's IAM Service Account Credentials emulation.
    /// </summary>
    /// <param name="config">The IAM Service Account Credentials configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithIamCredentials(IamCredentialsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Managed Service for Apache Kafka emulation.
    /// </summary>
    /// <param name="config">The Managed Service for Apache Kafka configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithKafka(KafkaConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud KMS emulation.
    /// </summary>
    /// <param name="config">The Cloud KMS configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithKms(GcpKmsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Logging emulation.
    /// </summary>
    /// <param name="config">The Cloud Logging configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithLogging(LoggingConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Monitoring emulation.
    /// </summary>
    /// <param name="config">The Cloud Monitoring configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithMonitoring(MonitoringConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Pub/Sub emulation.
    /// </summary>
    /// <param name="config">The Pub/Sub configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithPubSub(PubSubConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Resource Manager emulation.
    /// </summary>
    /// <param name="config">The Resource Manager configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithResourceManager(ResourceManagerConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Cloud Scheduler emulation.
    /// </summary>
    /// <param name="config">The Cloud Scheduler configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithScheduler(GcpSchedulerConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Secret Manager emulation.
    /// </summary>
    /// <param name="config">The Secret Manager configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithSecretManager(SecretManagerConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Service Usage emulation.
    /// </summary>
    /// <param name="config">The Service Usage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithServiceUsage(ServiceUsageConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Security Token Service emulation.
    /// </summary>
    /// <param name="config">The Security Token Service configuration.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithSts(StsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Sets the project used when a request names none (<c>FLOCI_GCP_DEFAULT_PROJECT_ID</c>). The
    /// emulator accepts any project id either way; this is also what
    /// <see cref="FlociGcpContainer.ProjectId" /> and <c>GOOGLE_CLOUD_PROJECT</c> in
    /// <see cref="FlociGcpContainer.GetEmulatorEnv" /> report.
    /// </summary>
    /// <param name="projectId">The project id.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithProjectId(string projectId)
    {
        return WithEnvironment(GcpCloud.ProjectIdEnv, projectId);
    }

    /// <summary>
    /// Serves HTTPS next to plain HTTP on the emulator port (<c>FLOCI_GCP_TLS_ENABLED</c>). Use
    /// <see cref="FlociGcpContainer.GetHttpsEndpoint" /> and trust
    /// <see cref="FlociGcpContainer.GetTlsCertificateAsync" /> in HTTPS clients. The container then
    /// counts as started only once it also answers its health path over HTTPS, which is slightly
    /// later than over HTTP. The <c>*_EMULATOR_HOST</c> variables keep using plain HTTP.
    /// </summary>
    /// <param name="enabled">Whether TLS is enabled.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithTls(bool enabled = true)
    {
        return WithEnvironment(GcpCloud.Descriptor.TlsEnabledEnv, enabled ? "true" : "false");
    }

    /// <summary>
    /// Sets the emulator's log level (e.g. <c>DEBUG</c>).
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithLogLevel(string level)
    {
        return WithEnvironment(GcpCloud.LogLevelEnv, level);
    }

    /// <summary>
    /// Always mounts the host Docker socket at <paramref name="hostSocketPath" />, whatever the
    /// enabled services need. By default the socket is mounted only when an enabled service
    /// spawns sibling containers (see <see cref="GcpCloud.Descriptor" />), decided from the
    /// container's final environment when <see cref="Build" /> runs.
    /// </summary>
    /// <param name="hostSocketPath">The host socket; defaults to <c>TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE</c> or <c>/var/run/docker.sock</c>.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithDockerSocket(string? hostSocketPath = null)
    {
        return Merge(DockerResourceConfiguration, new FlociGcpConfiguration(dockerSocket: hostSocketPath ?? FlociHost.HostDockerSocket()));
    }

    /// <summary>
    /// Never mounts the host Docker socket, even when an enabled service would need it.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithoutDockerSocket()
    {
        return Merge(DockerResourceConfiguration, new FlociGcpConfiguration(dockerSocket: string.Empty));
    }

    /// <summary>
    /// Sets the prefix of the sibling containers' names. Without it, each <see cref="Build" /> picks
    /// a unique <c>tc-xxxxxxxx</c> one, so containers never share sibling names, even when built
    /// from the same builder.
    /// </summary>
    /// <param name="resourceNamespace">The namespace.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    public FlociGcpBuilder WithResourceNamespace(string resourceNamespace)
    {
        return WithEnvironment(GcpCloud.Descriptor.ResourceNamespaceEnv, resourceNamespace);
    }

    /// <summary>
    /// Gets the container environment as it stands.
    /// </summary>
    internal IReadOnlyDictionary<string, string> Environments =>
        DockerResourceConfiguration.ServiceConfigs.Environment(DockerResourceConfiguration.Environments);

    /// <summary>
    /// Returns this builder with the environment variables and fixed host ports of its service
    /// configs written to the container configuration. <see cref="Build" /> calls it first; until
    /// then they are only kept, so that a config can still be replaced whole. Variables set with
    /// <c>WithEnvironment</c> are left as they are.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    internal FlociGcpBuilder WithServiceSettings()
    {
        var builder = this;
        var explicitEnvironment = DockerResourceConfiguration.Environments;
        foreach (var entry in Environments)
        {
            if (!explicitEnvironment.ContainsKey(entry.Key))
            {
                builder = builder.WithEnvironment(entry.Key, entry.Value);
            }
        }

        foreach (var port in DockerResourceConfiguration.ServiceConfigs.FixedHostPorts)
        {
            builder = builder.WithPortBinding(port, port);
        }

        return builder;
    }

    /// <summary>
    /// Gets the resource namespace set so far, or <see langword="null" /> when <see cref="Build" />
    /// will pick a fresh one.
    /// </summary>
    internal string? ResourceNamespace =>
        Environments.TryGetValue(GcpCloud.Descriptor.ResourceNamespaceEnv, out var ns) ? ns : null;

    /// <summary>
    /// Returns this builder with a fresh resource namespace unless one is already set (through
    /// <see cref="WithResourceNamespace" /> or <c>WithEnvironment</c>). <see cref="Build" /> calls it,
    /// so every built container gets its own.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    internal FlociGcpBuilder WithDefaultResourceNamespace()
    {
        return ResourceNamespace is null ? WithResourceNamespace(CloudDescriptor.NewNamespace()) : this;
    }

    /// <summary>
    /// Gets the wait strategies as they stand.
    /// </summary>
    internal IEnumerable<WaitStrategy> WaitStrategies => DockerResourceConfiguration.WaitStrategies;

    /// <summary>
    /// Returns this builder, also waiting for HTTPS when TLS is enabled in the environment as it
    /// stands (<c>FLOCI_GCP_TLS_ENABLED=true</c>). <see cref="Build" /> calls it; the wait is added to
    /// the configured strategies, not put in their place.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    internal FlociGcpBuilder WithTlsReadiness()
    {
        var current = DockerResourceConfiguration.WaitStrategies;
        var wanted = FlociReadiness.WithHttpsWhenTlsEnabled(GcpCloud.Descriptor, Environments, current);
        return ReferenceEquals(wanted, current) ? this : Clone(new ContainerConfiguration(waitStrategies: wanted));
    }

    /// <summary>
    /// Gets the host Docker socket the built container will mount, or <see langword="null" />.
    /// </summary>
    /// <returns>The host socket path, or <see langword="null" /> when none is mounted.</returns>
    internal string? DockerSocketMount()
    {
        var forced = DockerResourceConfiguration.DockerSocket;
        if (forced is not null)
        {
            return forced.Length == 0 ? null : forced;
        }

        return GcpCloud.Descriptor.DockerSocketRequired(Environments)
            ? FlociHost.HostDockerSocket()
            : null;
    }

    /// <inheritdoc />
    public override FlociGcpContainer Build()
    {
        Validate();
        var builder = WithServiceSettings().WithDefaultResourceNamespace().WithTlsReadiness();
        var socket = builder.DockerSocketMount();
        builder = socket is null ? builder : builder.WithBindMount(socket, FlociHost.ContainerDockerSocket);
        return new FlociGcpContainer(builder.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    /// <remarks>
    /// One setting is always set, and a later <c>WithEnvironment</c> overrides it:
    /// <c>FLOCI_GCP_TLS_HTTPS_PORT=0</c>, since the default extra HTTPS port 443 cannot be bound by
    /// an unprivileged container. The GKE endpoint mode is left at the emulator's default,
    /// <c>host</c>, which already advertises a cluster endpoint published on the Docker host.
    /// </remarks>
    protected override FlociGcpBuilder Init()
    {
        return base.Init()
            .WithPortBinding(FlociGcpPort, true)
            .WithEnvironment(GcpCloud.Descriptor.EnvPrefix + "TLS_HTTPS_PORT", "0")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPath(GcpCloud.Descriptor.HealthPath).ForPort(FlociGcpPort)));
    }

    /// <summary>
    /// Applies a per-service configuration by translating it to the corresponding
    /// <c>FLOCI_GCP_SERVICES_*</c> environment variables.
    /// </summary>
    /// <param name="config">The service configuration to apply.</param>
    /// <returns>A configured instance of <see cref="FlociGcpBuilder" />.</returns>
    private FlociGcpBuilder WithServiceConfig(FlociServiceConfig config)
    {
        return Merge(DockerResourceConfiguration, new FlociGcpConfiguration(FlociServiceConfigs.Empty.With(config)));
    }

    /// <inheritdoc />
    protected override FlociGcpBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociGcpConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociGcpBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociGcpConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociGcpBuilder Merge(FlociGcpConfiguration oldValue, FlociGcpConfiguration newValue)
    {
        return new FlociGcpBuilder(new FlociGcpConfiguration(oldValue, newValue));
    }
}
