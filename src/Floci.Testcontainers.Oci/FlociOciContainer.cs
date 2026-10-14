using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class FlociOciContainer : DockerContainer
{
    private readonly FlociOciConfiguration _configuration;

    private readonly Lazy<string> _privateKeyPem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociOciContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public FlociOciContainer(FlociOciConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
        _privateKeyPem = new Lazy<string>(
            () => configuration.PrivateKeyPem ?? RsaKeyPem.Generate(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets the emulator's base URL, which serves every OCI service. Set it as the endpoint of
    /// each SDK client (<c>client.SetEndpoint(...)</c>, followed by
    /// <c>client.SetRealmSpecificEndpointTemplate(null)</c> so the OCI SDK for .NET does not fall
    /// back to the region's real endpoint) or pass it to the CLI's <c>--endpoint</c>.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://localhost:32781</c> (no trailing slash).</returns>
    public string GetEndpoint() =>
        FlociHost.BaseUrl(Uri.UriSchemeHttp, Hostname, GetMappedPublicPort(FlociOciBuilder.FlociOciPort));

    /// <summary>
    /// Gets the emulator's region: <c>us-ashburn-1</c>, unless <see cref="FlociOciBuilder.WithRegion" />
    /// set another.
    /// </summary>
    public string Region => Setting(OciCloud.RegionEnv) ?? OciCloud.DefaultRegion;

    /// <summary>
    /// Gets the OCID of the tenancy, which is also its root compartment: the emulator's default
    /// one, unless <see cref="FlociOciBuilder.WithTenancyId" /> set another. The emulator keeps
    /// resources per tenancy and takes the tenancy from the request signature.
    /// </summary>
    public string TenancyId => Setting(OciCloud.TenancyIdEnv) ?? OciCloud.DefaultTenancyId;

    /// <summary>
    /// Gets the Object Storage namespace of the tenancy: <c>floci-local</c>, unless
    /// <see cref="FlociOciBuilder.WithNamespace" /> set another.
    /// </summary>
    public string Namespace => Setting(OciCloud.NamespaceEnv) ?? OciCloud.DefaultNamespace;

    /// <summary>
    /// Gets the user OCID to sign requests with. Any OCID works; the emulator has no user check.
    /// </summary>
    public string UserId => OciCloud.DefaultUserId;

    /// <summary>
    /// Gets the API key fingerprint to sign requests with. It is a fixed placeholder, not the
    /// fingerprint of <see cref="GetPrivateKeyPem" />: the emulator parses the signature for the
    /// tenancy and user but never verifies it.
    /// </summary>
    public string Fingerprint => OciCloud.DefaultFingerprint;

    /// <summary>
    /// Gets an RSA private key to sign requests with, as the OCI SDKs and CLI require a key even
    /// though the emulator never verifies the signature. Unless
    /// <see cref="FlociOciBuilder.WithPrivateKeyPem" /> supplied one, it is a throwaway 2048-bit key
    /// generated on first use, once per container object, that never leaves the test process.
    /// </summary>
    /// <returns>The key as a PEM document (PKCS#1 when generated).</returns>
    public string GetPrivateKeyPem() => _privateKeyPem.Value;

    /// <summary>
    /// Gets the contents of an OCI config file (<c>~/.oci/config</c> format) with one profile that
    /// carries this container's user, fingerprint, tenancy and region. Write
    /// <see cref="GetPrivateKeyPem" /> to <paramref name="keyFilePath" /> and this text to a file,
    /// then load it with the SDK's config-file provider or <c>oci --config-file</c>. The endpoint
    /// is not part of a config file: set <see cref="GetEndpoint" /> on each client.
    /// </summary>
    /// <param name="keyFilePath">The path the private key is, or will be, stored at.</param>
    /// <param name="profile">The profile name.</param>
    /// <returns>The config file contents.</returns>
    public string GetConfigFileContents(string keyFilePath, string profile = "DEFAULT")
    {
        return "[" + profile + "]\n"
            + "user=" + UserId + "\n"
            + "fingerprint=" + Fingerprint + "\n"
            + "tenancy=" + TenancyId + "\n"
            + "region=" + Region + "\n"
            + "key_file=" + keyFilePath + "\n";
    }

    /// <summary>
    /// Gets the prefix of the sibling containers' names (<c>FLOCI_OCI_DOCKER_RESOURCE_NAMESPACE</c>).
    /// </summary>
    public string? ResourceNamespace => Setting(OciCloud.Descriptor.ResourceNamespaceEnv);

    /// <summary>
    /// Wipes all emulator state (buckets, queues, keys, ...) without restarting the container.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the state is reset.</returns>
    public Task ResetAsync(CancellationToken ct = default)
    {
        return FlociHost.ResetAsync(OciCloud.Descriptor, GetEndpoint(), ct);
    }

    /// <summary>
    /// Stops and removes the emulator, then removes the sibling containers it spawned (those
    /// labelled with its <see cref="ResourceNamespace" />). Containers sharing a namespace set
    /// through <see cref="FlociOciBuilder.WithResourceNamespace" /> lose their siblings too.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
        finally
        {
            var resourceNamespace = ResourceNamespace;
            if (!string.IsNullOrEmpty(resourceNamespace))
            {
                await FlociHost.RemoveSiblingContainersAsync(_configuration.DockerEndpointAuthConfig, resourceNamespace!)
                    .ConfigureAwait(false);
            }
        }
    }

    private string? Setting(string name) =>
        _configuration.Environments.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value) ? value : null;
}
