using System.Collections.Generic;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
/// <remarks>
/// <code>
/// await using var floci = new FlociOciBuilder("floci/floci-oci:latest").Build();
/// await floci.StartAsync();
/// File.WriteAllText(keyFile, floci.GetPrivateKeyPem());
/// File.WriteAllText(configFile, floci.GetConfigFileContents(keyFile));
/// var client = new ObjectStorageClient(new ConfigFileAuthenticationDetailsProvider(configFile, "DEFAULT"));
/// client.SetEndpoint(floci.GetEndpoint());
/// client.SetRealmSpecificEndpointTemplate(null);
/// </code>
/// </remarks>
[PublicAPI]
public sealed class FlociOciBuilder : ContainerBuilder<FlociOciBuilder, FlociOciContainer, FlociOciConfiguration>
{
    /// <summary>
    /// The single edge port that Floci OCI exposes all OCI services on.
    /// </summary>
    public const ushort FlociOciPort = 4599;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full Docker image name, including repository and tag (e.g. <c>floci/floci-oci:x.y.z</c>).
    /// This also lets you point at a private registry mirror.
    /// </param>
    public FlociOciBuilder(string image)
        : this(new DockerImage(image))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciBuilder" /> class.
    /// </summary>
    /// <param name="image">The Docker image to use for the container.</param>
    public FlociOciBuilder(IImage image)
        : this(new FlociOciConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private FlociOciBuilder(FlociOciConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override FlociOciConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Configures Floci's Functions emulation.
    /// </summary>
    /// <param name="config">The Functions configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithFunctions(OciFunctionsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Identity and Access Management emulation.
    /// </summary>
    /// <param name="config">The Identity and Access Management configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithIdentity(IdentityConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Key Management emulation.
    /// </summary>
    /// <param name="config">The Key Management configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithKms(OciKmsConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Object Storage emulation.
    /// </summary>
    /// <param name="config">The Object Storage configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithObjectStorage(ObjectStorageConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Kubernetes Engine (OKE) emulation.
    /// </summary>
    /// <param name="config">The Kubernetes Engine (OKE) configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithOke(OkeConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Queue emulation.
    /// </summary>
    /// <param name="config">The Queue configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithQueue(OciQueueConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Streaming emulation.
    /// </summary>
    /// <param name="config">The Streaming configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithStreaming(StreamingConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Configures Floci's Vault emulation.
    /// </summary>
    /// <param name="config">The Vault configuration.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithVault(VaultConfig config) => WithServiceConfig(config);

    /// <summary>
    /// Sets the emulator's region (<c>FLOCI_OCI_DEFAULT_REGION</c>), which
    /// <see cref="FlociOciContainer.Region" /> and the config-file profile then report.
    /// </summary>
    /// <param name="region">The region identifier, e.g. <c>eu-frankfurt-1</c>.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithRegion(string region)
    {
        return WithEnvironment(OciCloud.RegionEnv, region);
    }

    /// <summary>
    /// Sets the tenancy the emulator uses for requests that carry no signature
    /// (<c>FLOCI_OCI_DEFAULT_TENANCY_ID</c>). It is also the tenancy of the credentials
    /// <see cref="FlociOciContainer" /> hands out, so signed and unsigned requests see the same
    /// resources.
    /// </summary>
    /// <param name="tenancyId">The tenancy OCID.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithTenancyId(string tenancyId)
    {
        return WithEnvironment(OciCloud.TenancyIdEnv, tenancyId);
    }

    /// <summary>
    /// Sets the Object Storage namespace of the emulated tenancy (<c>FLOCI_OCI_DEFAULT_NAMESPACE</c>).
    /// </summary>
    /// <param name="objectStorageNamespace">The namespace.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithNamespace(string objectStorageNamespace)
    {
        return WithEnvironment(OciCloud.NamespaceEnv, objectStorageNamespace);
    }

    /// <summary>
    /// Hands out <paramref name="privateKeyPem" /> from <see cref="FlociOciContainer.GetPrivateKeyPem" />
    /// instead of a generated key, e.g. to sign with a key the code under test already has. The
    /// emulator never verifies signatures, so any RSA key works.
    /// </summary>
    /// <param name="privateKeyPem">The PEM-encoded RSA private key.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithPrivateKeyPem(string privateKeyPem)
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(privateKeyPem: privateKeyPem));
    }

    /// <summary>
    /// Gets the private key set through <see cref="WithPrivateKeyPem" />, or <see langword="null" />.
    /// </summary>
    internal string? PrivateKeyPem => DockerResourceConfiguration.PrivateKeyPem;

    /// <summary>
    /// Sets the emulator's log level (e.g. <c>DEBUG</c>).
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithLogLevel(string level)
    {
        return WithEnvironment(OciCloud.LogLevelEnv, level);
    }

    /// <summary>
    /// Always mounts the host Docker socket at <paramref name="hostSocketPath" />, whatever the
    /// enabled services need. By default the socket is mounted only when an enabled service
    /// spawns sibling containers (see <see cref="OciCloud.Descriptor" />), decided from the
    /// container's final environment when <see cref="Build" /> runs.
    /// </summary>
    /// <param name="hostSocketPath">The host socket; defaults to <c>TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE</c> or <c>/var/run/docker.sock</c>.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithDockerSocket(string? hostSocketPath = null)
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(dockerSocket: hostSocketPath ?? FlociHost.HostDockerSocket()));
    }

    /// <summary>
    /// Never mounts the host Docker socket, even when an enabled service would need it.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithoutDockerSocket()
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(dockerSocket: string.Empty));
    }

    /// <summary>
    /// Sets the prefix of the sibling containers' names. Without it, each <see cref="Build" /> picks
    /// a unique <c>tc-xxxxxxxx</c> one, so containers never share sibling names, even when built
    /// from the same builder.
    /// </summary>
    /// <param name="resourceNamespace">The namespace.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    public FlociOciBuilder WithResourceNamespace(string resourceNamespace)
    {
        return WithEnvironment(OciCloud.Descriptor.ResourceNamespaceEnv, resourceNamespace);
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
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    internal FlociOciBuilder WithServiceSettings()
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
        Environments.TryGetValue(OciCloud.Descriptor.ResourceNamespaceEnv, out var ns) ? ns : null;

    /// <summary>
    /// Returns this builder with a fresh resource namespace unless one is already set (through
    /// <see cref="WithResourceNamespace" /> or <c>WithEnvironment</c>). <see cref="Build" /> calls it,
    /// so every built container gets its own.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    internal FlociOciBuilder WithDefaultResourceNamespace()
    {
        return ResourceNamespace is null ? WithResourceNamespace(CloudDescriptor.NewNamespace()) : this;
    }

    /// <summary>
    /// Gets the wait strategies as they stand.
    /// </summary>
    internal IEnumerable<WaitStrategy> WaitStrategies => DockerResourceConfiguration.WaitStrategies;

    /// <summary>
    /// Returns this builder, also waiting for HTTPS when TLS is enabled in the environment as it
    /// stands (<c>FLOCI_OCI_TLS_ENABLED=true</c>). <see cref="Build" /> calls it; the wait is added to
    /// the configured strategies, not put in their place.
    /// </summary>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    internal FlociOciBuilder WithTlsReadiness()
    {
        var current = DockerResourceConfiguration.WaitStrategies;
        var wanted = FlociReadiness.WithHttpsWhenTlsEnabled(OciCloud.Descriptor, Environments, current);
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

        return OciCloud.Descriptor.DockerSocketRequired(Environments)
            ? FlociHost.HostDockerSocket()
            : null;
    }

    /// <inheritdoc />
    public override FlociOciContainer Build()
    {
        Validate();
        var builder = WithServiceSettings().WithDefaultResourceNamespace().WithTlsReadiness();
        var socket = builder.DockerSocketMount();
        builder = socket is null ? builder : builder.WithBindMount(socket, FlociHost.ContainerDockerSocket);
        return new FlociOciContainer(builder.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    /// <remarks>
    /// One setting is always set, and a later <c>WithEnvironment</c> overrides it:
    /// <c>FLOCI_OCI_TLS_HTTPS_PORT=0</c>, since the default extra HTTPS port 443 cannot be bound by
    /// an unprivileged container.
    /// </remarks>
    protected override FlociOciBuilder Init()
    {
        return base.Init()
            .WithPortBinding(FlociOciPort, true)
            .WithEnvironment(OciCloud.Descriptor.EnvPrefix + "TLS_HTTPS_PORT", "0")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPath(OciCloud.Descriptor.HealthPath).ForPort(FlociOciPort)));
    }

    /// <summary>
    /// Applies a per-service configuration by translating it to the corresponding
    /// <c>FLOCI_OCI_SERVICES_*</c> environment variables.
    /// </summary>
    /// <param name="config">The service configuration to apply.</param>
    /// <returns>A configured instance of <see cref="FlociOciBuilder" />.</returns>
    private FlociOciBuilder WithServiceConfig(FlociServiceConfig config)
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(FlociServiceConfigs.Empty.With(config)));
    }

    /// <inheritdoc />
    protected override FlociOciBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociOciBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new FlociOciConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override FlociOciBuilder Merge(FlociOciConfiguration oldValue, FlociOciConfiguration newValue)
    {
        return new FlociOciBuilder(new FlociOciConfiguration(oldValue, newValue));
    }
}
