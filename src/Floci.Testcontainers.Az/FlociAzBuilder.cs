using System.Collections.Generic;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
/// <remarks>
/// <code>
/// await using var floci = new FlociAzBuilder("floci/floci-az:latest").Build();
/// await floci.StartAsync();
/// var blobs = new BlobServiceClient(floci.GetStorageConnectionString());
/// </code>
/// </remarks>
[PublicAPI]
public sealed class FlociAzBuilder : ContainerBuilder<FlociAzBuilder, FlociAzContainer, FlociAzConfiguration>
{
    /// <summary>
    /// The single edge port that Floci Azure exposes all Azure services on.
    /// </summary>
    public const ushort FlociAzPort = 4577;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full Docker image name, including repository and tag (e.g. <c>floci/floci-az:x.y.z</c>).
    /// This also lets you point at a private registry mirror.
    /// </param>
    public FlociAzBuilder(string image)
        : this(new DockerImage(image))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzBuilder" /> class.
    /// </summary>
    /// <param name="image">The Docker image to use for the container.</param>
    public FlociAzBuilder(IImage image)
        : this(new FlociAzConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private FlociAzBuilder(FlociAzConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override FlociAzConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Configures Floci's Azure Container Instances emulation.
    /// </summary>
    /// <param name="config">The Azure Container Instances configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithAci(AciConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Container Registry emulation.
    /// </summary>
    /// <param name="config">The Azure Container Registry configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithAcr(AcrConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Kubernetes Service (AKS) emulation.
    /// </summary>
    /// <param name="config">The Azure Kubernetes Service (AKS) configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithAks(AksConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure API Management emulation.
    /// </summary>
    /// <param name="config">The Azure API Management configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithApim(ApimConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure App Configuration emulation.
    /// </summary>
    /// <param name="config">The Azure App Configuration configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithAppConfiguration(AppConfigurationConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Resource Manager (ARM) emulation.
    /// </summary>
    /// <param name="config">The Azure Resource Manager (ARM) configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithArm(ArmConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Blob Storage emulation.
    /// </summary>
    /// <param name="config">The Azure Blob Storage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithBlob(BlobConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Container Apps emulation.
    /// </summary>
    /// <param name="config">The Azure Container Apps configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithContainerApps(ContainerAppsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Cosmos DB emulation.
    /// </summary>
    /// <param name="config">The Azure Cosmos DB configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithCosmos(CosmosConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Communication Services Email emulation.
    /// </summary>
    /// <param name="config">The Azure Communication Services Email configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithEmail(EmailConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Microsoft Entra ID emulation.
    /// </summary>
    /// <param name="config">The Microsoft Entra ID configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithEntra(EntraConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Event Grid emulation.
    /// </summary>
    /// <param name="config">The Azure Event Grid configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithEventGrid(EventGridConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Event Hubs emulation.
    /// </summary>
    /// <param name="config">The Azure Event Hubs configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithEventHub(EventHubConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Functions emulation.
    /// </summary>
    /// <param name="config">The Azure Functions configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithFunctions(FunctionsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Microsoft Graph emulation.
    /// </summary>
    /// <param name="config">The Microsoft Graph configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithGraph(GraphConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Key Vault emulation.
    /// </summary>
    /// <param name="config">The Azure Key Vault configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithKeyVault(KeyVaultConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Managed Identity emulation.
    /// </summary>
    /// <param name="config">The Azure Managed Identity configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithManagedIdentity(ManagedIdentityConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Database for MariaDB emulation.
    /// </summary>
    /// <param name="config">The Azure Database for MariaDB configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithMariaDb(MariaDbConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Monitor / Log Analytics emulation.
    /// </summary>
    /// <param name="config">The Azure Monitor / Log Analytics configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithMonitor(MonitorConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Database for MySQL emulation.
    /// </summary>
    /// <param name="config">The Azure Database for MySQL configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithMySql(MySqlConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Virtual Network emulation.
    /// </summary>
    /// <param name="config">The Azure Virtual Network configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithNetwork(NetworkConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Policy emulation.
    /// </summary>
    /// <param name="config">The Azure Policy configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithPolicy(PolicyConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Database for PostgreSQL emulation.
    /// </summary>
    /// <param name="config">The Azure Database for PostgreSQL configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithPostgres(PostgresConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Queue Storage emulation.
    /// </summary>
    /// <param name="config">The Azure Queue Storage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithQueue(QueueConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Cache for Redis emulation.
    /// </summary>
    /// <param name="config">The Azure Cache for Redis configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithRedis(RedisConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Service Bus emulation.
    /// </summary>
    /// <param name="config">The Azure Service Bus configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithServiceBus(ServiceBusConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure SignalR Service emulation.
    /// </summary>
    /// <param name="config">The Azure SignalR Service configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithSignalR(SignalRConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure SQL Database emulation.
    /// </summary>
    /// <param name="config">The Azure SQL Database configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithSql(SqlConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Table Storage emulation.
    /// </summary>
    /// <param name="config">The Azure Table Storage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithTable(TableConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Azure Virtual Machines emulation.
    /// </summary>
    /// <param name="config">The Azure Virtual Machines configuration.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithVm(VmConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Serves HTTPS next to plain HTTP on the emulator port (<c>FLOCI_AZ_TLS_ENABLED</c>). Use
    /// <see cref="FlociAzContainer.GetHttpsEndpoint" /> and trust
    /// <see cref="FlociAzContainer.GetTlsCertificateAsync" /> in HTTPS clients. The container then
    /// counts as started only once it also answers its health path over HTTPS, which is slightly
    /// later than over HTTP.
    /// </summary>
    /// <param name="enabled">Whether TLS is enabled.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithTls(bool enabled = true)
    {
        return WithEnvironment(AzCloud.Descriptor.TlsEnabledEnv, enabled ? "true" : "false");
    }

    /// <summary>
    /// Sets the emulator's log level (e.g. <c>DEBUG</c>).
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithLogLevel(string level)
    {
        return WithEnvironment(AzCloud.LogLevelEnv, level);
    }

    /// <summary>
    /// Always mounts the host Docker socket at <paramref name="hostSocketPath" />, whatever the
    /// enabled services need. By default the socket is mounted only when an enabled service
    /// spawns sibling containers (see <see cref="AzCloud.Descriptor" />), decided from the
    /// container's final environment when <see cref="Build" /> runs.
    /// </summary>
    /// <param name="hostSocketPath">The host socket; defaults to <c>TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE</c> or <c>/var/run/docker.sock</c>.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithDockerSocket(string? hostSocketPath = null)
    {
        return Merge(DockerResourceConfiguration, new FlociAzConfiguration(dockerSocket: hostSocketPath ?? FlociHost.HostDockerSocket()));
    }

    /// <summary>
    /// Never mounts the host Docker socket, even when an enabled service would need it.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithoutDockerSocket()
    {
        return Merge(DockerResourceConfiguration, new FlociAzConfiguration(dockerSocket: string.Empty));
    }

    /// <summary>
    /// Sets the prefix of the sibling containers' names. Without it, each <see cref="Build" /> picks
    /// a unique <c>tc-xxxxxxxx</c> one, so containers never share sibling names, even when built
    /// from the same builder.
    /// </summary>
    /// <param name="resourceNamespace">The namespace.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    public FlociAzBuilder WithResourceNamespace(string resourceNamespace)
    {
        return WithEnvironment(AzCloud.Descriptor.ResourceNamespaceEnv, resourceNamespace);
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
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    internal FlociAzBuilder WithServiceSettings()
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
        Environments.TryGetValue(AzCloud.Descriptor.ResourceNamespaceEnv, out var ns) ? ns : null;

    /// <summary>
    /// Returns this builder with a fresh resource namespace unless one is already set (through
    /// <see cref="WithResourceNamespace" /> or <c>WithEnvironment</c>). <see cref="Build" /> calls it,
    /// so every built container gets its own.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    internal FlociAzBuilder WithDefaultResourceNamespace()
    {
        return ResourceNamespace is null ? WithResourceNamespace(CloudDescriptor.NewNamespace()) : this;
    }

    /// <summary>
    /// Gets the wait strategies as they stand.
    /// </summary>
    internal IEnumerable<WaitStrategy> WaitStrategies => DockerResourceConfiguration.WaitStrategies;

    /// <summary>
    /// Returns this builder, also waiting for HTTPS when TLS is enabled in the environment as it
    /// stands (through <see cref="WithTls" /> or <c>WithEnvironment</c>). <see cref="Build" /> calls
    /// it; the wait is added to the configured strategies, not put in their place.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    internal FlociAzBuilder WithTlsReadiness()
    {
        var current = DockerResourceConfiguration.WaitStrategies;
        var wanted = FlociReadiness.WithHttpsWhenTlsEnabled(AzCloud.Descriptor, Environments, current);
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

        return AzCloud.Descriptor.DockerSocketRequired(Environments)
            ? FlociHost.HostDockerSocket()
            : null;
    }

    /// <inheritdoc />
    public override FlociAzContainer Build()
    {
        Validate();
        var builder = WithServiceSettings().WithDefaultResourceNamespace().WithTlsReadiness();
        var socket = builder.DockerSocketMount();
        builder = socket is null ? builder : builder.WithBindMount(socket, FlociHost.ContainerDockerSocket);
        return new FlociAzContainer(builder.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Two settings are always set, and a later <c>WithEnvironment</c> overrides either:
    /// <c>FLOCI_AZ_DOCKER_ENDPOINT_MODE=published</c>, so the host and port the emulator reports for
    /// its sibling containers are reachable from the test process, and
    /// <c>FLOCI_AZ_TLS_HTTPS_PORT=0</c>, since the default extra HTTPS port 443 cannot be bound by
    /// an unprivileged container.
    /// </remarks>
    protected override FlociAzBuilder Init()
    {
        return base.Init()
            .WithPortBinding(FlociAzPort, true)
            .WithEnvironment(AzCloud.Descriptor.EnvPrefix + "DOCKER_ENDPOINT_MODE", "published")
            .WithEnvironment(AzCloud.Descriptor.EnvPrefix + "TLS_HTTPS_PORT", "0")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPath(AzCloud.Descriptor.HealthPath).ForPort(FlociAzPort)));
    }

    /// <summary>
    /// Applies a per-service configuration by translating it to the corresponding
    /// <c>FLOCI_AZ_SERVICES_*</c> environment variables.
    /// </summary>
    /// <param name="config">The service configuration to apply.</param>
    /// <returns>A configured instance of <see cref="FlociAzBuilder" />.</returns>
    private FlociAzBuilder WithServiceConfig(FlociServiceConfig config)
    {
        return Merge(DockerResourceConfiguration, new FlociAzConfiguration(FlociServiceConfigs.Empty.With(config)));
    }

    /// <inheritdoc />
    protected override FlociAzBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociAzConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociAzBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociAzConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociAzBuilder Merge(FlociAzConfiguration oldValue, FlociAzConfiguration newValue)
    {
        return new FlociAzBuilder(new FlociAzConfiguration(oldValue, newValue));
    }
}
