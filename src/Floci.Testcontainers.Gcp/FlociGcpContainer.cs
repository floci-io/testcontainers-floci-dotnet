using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class FlociGcpContainer : DockerContainer
{
    private const string TlsCertificatePath = "/_floci-gcp/tls-cert";

    private static readonly HttpClient Http = new();

    private readonly FlociGcpConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociGcpContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public FlociGcpContainer(FlociGcpConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the emulator's base URL, which serves every Google Cloud service over REST. The same
    /// host and port serve gRPC in plain text.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://localhost:32781</c> (no trailing slash).</returns>
    public string GetEndpoint() => Uri.UriSchemeHttp + "://" + GetHostAndPort();

    /// <summary>
    /// Gets the emulator's HTTPS base URL. The emulator serves HTTPS on the same port as plain
    /// HTTP, but only while TLS is enabled (<see cref="FlociGcpBuilder.WithTls" />). Clients have to
    /// trust the certificate <see cref="GetTlsCertificateAsync" /> returns.
    /// </summary>
    /// <returns>The endpoint, e.g. <c>https://localhost:32781</c> (no trailing slash).</returns>
    public string GetHttpsEndpoint() => Uri.UriSchemeHttps + "://" + GetHostAndPort();

    /// <summary>
    /// Gets the emulator's address without a scheme, the form gRPC clients and most
    /// <c>*_EMULATOR_HOST</c> variables take.
    /// </summary>
    /// <returns>The address, e.g. <c>localhost:32781</c>; an IPv6 host is in brackets (<c>[::1]:32781</c>).</returns>
    public string GetHostAndPort() => FlociHost.HostAndPort(Hostname, GetMappedPublicPort(FlociGcpBuilder.FlociGcpPort));

    /// <summary>
    /// Fetches the PEM-encoded certificate the emulator serves HTTPS with, to import into the
    /// trust store of HTTPS clients.
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
                $"TLS certificate not available (HTTP {(int)response.StatusCode}): {body}. Enable TLS with FlociGcpBuilder.WithTls().");
        }

        return body;
    }

    /// <summary>
    /// Gets the id of the default project: <c>floci-local</c>, unless
    /// <see cref="FlociGcpBuilder.WithProjectId" /> set another. The emulator accepts any project id.
    /// </summary>
    public string ProjectId => Setting(GcpCloud.ProjectIdEnv) ?? GcpCloud.DefaultProjectId;

    /// <summary>
    /// Gets the environment variables that point the Google Cloud client libraries at this
    /// container: the <c>*_EMULATOR_HOST</c> variable of every service that has one, plus
    /// <c>GOOGLE_CLOUD_PROJECT</c>. Set them on the process under test (or on the test process,
    /// e.g. with <see cref="Environment.SetEnvironmentVariable(string, string)" />) before creating
    /// clients; libraries that see them skip credentials and TLS.
    /// </summary>
    /// <remarks>
    /// All values are <c>host:port</c> except <c>STORAGE_EMULATOR_HOST</c>, which the Cloud Storage
    /// libraries expect as a URL (<c>http://host:port</c>). <c>LOGGING_EMULATOR_HOST</c> and
    /// <c>KMS_EMULATOR_HOST</c> are a floci-gcp convention that no Google client library reads:
    /// they are there for your own code to pick the endpoint up from. Services without such a variable
    /// (BigQuery, Cloud Tasks, Cloud Monitoring, ...) take <see cref="GetEndpoint" /> as an
    /// explicit endpoint instead.
    /// </remarks>
    /// <returns>The variables, by name.</returns>
    public IReadOnlyDictionary<string, string> GetEmulatorEnv()
    {
        var hostAndPort = GetHostAndPort();
        return new Dictionary<string, string>
        {
            ["PUBSUB_EMULATOR_HOST"] = hostAndPort,
            ["FIRESTORE_EMULATOR_HOST"] = hostAndPort,
            ["DATASTORE_EMULATOR_HOST"] = hostAndPort,
            ["SECRET_MANAGER_EMULATOR_HOST"] = hostAndPort,
            ["FIREBASE_AUTH_EMULATOR_HOST"] = hostAndPort,
            ["LOGGING_EMULATOR_HOST"] = hostAndPort,
            ["KMS_EMULATOR_HOST"] = hostAndPort,
            ["STORAGE_EMULATOR_HOST"] = GetEndpoint(),
            ["STORAGE_EMULATOR_HOST_GRPC"] = hostAndPort,
            ["GOOGLE_CLOUD_PROJECT"] = ProjectId,
        };
    }

    /// <summary>
    /// Gets the prefix of the sibling containers' names (<c>FLOCI_GCP_DOCKER_RESOURCE_NAMESPACE</c>).
    /// </summary>
    public string? ResourceNamespace => Setting(GcpCloud.Descriptor.ResourceNamespaceEnv);

    /// <summary>
    /// Wipes all emulator state (buckets, topics, documents, ...) without restarting the container.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the state is reset.</returns>
    public Task ResetAsync(CancellationToken ct = default)
    {
        return FlociHost.ResetAsync(GcpCloud.Descriptor, GetEndpoint(), ct);
    }

    /// <summary>
    /// Stops and removes the emulator, then removes the sibling containers it spawned (those
    /// labelled with its <see cref="ResourceNamespace" />). Containers sharing a namespace set
    /// through <see cref="FlociGcpBuilder.WithResourceNamespace" /> lose their siblings too.
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
