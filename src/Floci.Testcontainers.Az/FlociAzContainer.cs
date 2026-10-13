using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class FlociAzContainer : DockerContainer
{
    private const string TlsCertificatePath = "/_floci/tls-cert";

    private static readonly HttpClient Http = new();

    private readonly FlociAzConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociAzContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public FlociAzContainer(FlociAzConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the emulator's base URL, which serves every Azure service.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://localhost:32781</c> (no trailing slash).</returns>
    public string GetEndpoint() => BaseUrl(Uri.UriSchemeHttp);

    /// <summary>
    /// Gets the emulator's HTTPS base URL. The emulator serves HTTPS on the same port as plain
    /// HTTP, but only while TLS is enabled (<see cref="FlociAzBuilder.WithTls" />). Clients have to
    /// trust the certificate <see cref="GetTlsCertificateAsync" /> returns.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>https://localhost:32781</c> (no trailing slash).</returns>
    public string GetHttpsEndpoint() => BaseUrl(Uri.UriSchemeHttps);

    /// <summary>
    /// Fetches the PEM-encoded certificate the emulator serves HTTPS with, to import into the
    /// trust store of HTTPS clients such as the Cosmos DB or Key Vault SDKs.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The PEM-encoded TLS certificate.</returns>
    /// <exception cref="InvalidOperationException">TLS is not enabled or the certificate is not available.</exception>
    public async Task<string> GetTlsCertificateAsync(CancellationToken ct = default)
    {
        using var response = await Http.GetAsync(new Uri(GetEndpoint() + TlsCertificatePath), ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"TLS certificate not available (HTTP {(int)response.StatusCode}): {body}. Enable TLS with FlociAzBuilder.WithTls().");
        }

        return body;
    }

    /// <summary>
    /// Gets the id of the default subscription, the one <c>GET /subscriptions</c> lists; other
    /// subscription ids are accepted as well.
    /// </summary>
    public string SubscriptionId => AzCloud.DefaultSubscriptionId;

    /// <summary>
    /// Gets the id of the default Microsoft Entra ID tenant.
    /// </summary>
    public string TenantId => AzCloud.DefaultTenantId;

    /// <summary>
    /// Gets the name of the default storage account. The emulator serves the data planes of all
    /// storage services under paths prefixed with it.
    /// </summary>
    public string AccountName => AzCloud.DefaultAccountName;

    /// <summary>
    /// Gets the base64-encoded key of the default storage account: the well-known development
    /// storage key, unless <c>FLOCI_AZ_AUTH_STORAGE_ACCOUNT_KEYS_DEVSTOREACCOUNT1</c> sets another.
    /// </summary>
    public string AccountKey =>
        Setting(AzCloud.Descriptor.EnvPrefix + "AUTH_STORAGE_ACCOUNT_KEYS_" + AzCloud.DefaultAccountName.ToUpperInvariant())
        ?? AzCloud.DefaultAccountKey;

    /// <summary>
    /// Gets the Blob Storage endpoint of the default storage account.
    /// </summary>
    /// <remarks>
    /// The storage endpoints address the emulator by IP (<c>127.0.0.1</c> for <c>localhost</c>).
    /// The Azure Storage SDK for .NET reads the account name from the first path segment only when
    /// the host is an IP address; with a host name it takes that segment for the container or queue.
    /// </remarks>
    /// <returns>The endpoint, e.g. <c>http://127.0.0.1:32781/devstoreaccount1</c>.</returns>
    public string GetBlobEndpoint() => StorageBaseUrl() + "/" + AccountName;

    /// <summary>
    /// Gets the Queue Storage endpoint of the default storage account. See <see cref="GetBlobEndpoint" />
    /// for why the host is an IP address.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://127.0.0.1:32781/devstoreaccount1-queue</c>.</returns>
    public string GetQueueEndpoint() => StorageBaseUrl() + "/" + AccountName + "-queue";

    /// <summary>
    /// Gets the Table Storage endpoint of the default storage account. See <see cref="GetBlobEndpoint" />
    /// for why the host is an IP address.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://127.0.0.1:32781/devstoreaccount1-table</c>.</returns>
    public string GetTableEndpoint() => StorageBaseUrl() + "/" + AccountName + "-table";

    /// <summary>
    /// Gets a storage connection string for the default storage account, with the Blob, Queue and
    /// Table endpoints pointing at this container.
    /// </summary>
    /// <returns>The storage connection string.</returns>
    public string GetStorageConnectionString()
    {
        return "DefaultEndpointsProtocol=http"
            + ";AccountName=" + AccountName
            + ";AccountKey=" + AccountKey
            + ";BlobEndpoint=" + GetBlobEndpoint()
            + ";QueueEndpoint=" + GetQueueEndpoint()
            + ";TableEndpoint=" + GetTableEndpoint()
            + ";";
    }

    /// <summary>
    /// Gets the prefix of the sibling containers' names (<c>FLOCI_AZ_DOCKER_RESOURCE_NAMESPACE</c>).
    /// </summary>
    public string? ResourceNamespace => Setting(AzCloud.Descriptor.ResourceNamespaceEnv);

    /// <summary>
    /// Wipes all emulator state (containers, queues, tables, ...) without restarting the container.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the state is reset.</returns>
    public Task ResetAsync(CancellationToken ct = default)
    {
        return FlociHost.ResetAsync(AzCloud.Descriptor, GetEndpoint(), ct);
    }

    /// <summary>
    /// Stops and removes the emulator, then removes the sibling containers it spawned (those
    /// labelled with its <see cref="ResourceNamespace" />). Containers sharing a namespace set
    /// through <see cref="FlociAzBuilder.WithResourceNamespace" /> lose their siblings too.
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

    private string BaseUrl(string scheme) => BaseUrl(scheme, Hostname);

    private string BaseUrl(string scheme, string host) =>
        scheme + "://" + host + ":" + GetMappedPublicPort(FlociAzBuilder.FlociAzPort).ToString(CultureInfo.InvariantCulture);

    private string StorageBaseUrl() => BaseUrl(Uri.UriSchemeHttp, IpHost(Hostname));

    /// <summary>
    /// Returns <paramref name="host" /> as an IPv4 address, in the form a URL takes: itself when it
    /// is an IP address already, the loopback address for <c>localhost</c>, otherwise what it
    /// resolves to. A host that does not resolve is returned unchanged.
    /// </summary>
    /// <param name="host">A host name or IP address.</param>
    /// <returns>The host to put in a storage endpoint.</returns>
    internal static string IpHost(string host)
    {
        if (IPAddress.TryParse(host, out var address))
        {
            return address.AddressFamily == AddressFamily.InterNetworkV6 ? "[" + address + "]" : host;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Loopback.ToString();
        }

        try
        {
            var resolved = Dns.GetHostAddresses(host).FirstOrDefault(candidate => candidate.AddressFamily == AddressFamily.InterNetwork);
            return resolved?.ToString() ?? host;
        }
        catch (SocketException)
        {
            return host;
        }
    }

    private string? Setting(string name) =>
        _configuration.Environments.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value) ? value : null;
}
