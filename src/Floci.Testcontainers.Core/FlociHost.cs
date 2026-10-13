using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Core;

/// <summary>
/// Host-side helpers shared by the Floci cloud packages: the Docker socket to mount and the
/// emulator's state reset.
/// </summary>
[PublicAPI]
public static class FlociHost
{
    /// <summary>The Docker socket path inside the emulator container.</summary>
    public const string ContainerDockerSocket = "/var/run/docker.sock";

    private static readonly HttpClient Http = new();

    /// <summary>
    /// The label Floci puts on every sibling container it spawns, holding the emulator's
    /// <c>DOCKER_RESOURCE_NAMESPACE</c>.
    /// </summary>
    public const string NamespaceLabel = "floci_namespace";

    /// <summary>
    /// Gets the host Docker socket to mount: <c>TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE</c> when set
    /// (Colima and other VMs expose the in-VM socket path there), otherwise
    /// <see cref="ContainerDockerSocket" />.
    /// </summary>
    /// <returns>The host socket path.</returns>
    public static string HostDockerSocket()
    {
        var socketPath = Environment.GetEnvironmentVariable("TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE");
        return string.IsNullOrEmpty(socketPath) ? ContainerDockerSocket : socketPath!;
    }

    /// <summary>
    /// Returns <paramref name="host" /> in the form a URL authority takes: an IPv6 literal in
    /// square brackets (<c>::1</c> becomes <c>[::1]</c>), anything else unchanged.
    /// </summary>
    /// <param name="host">A host name or IP address, as Testcontainers reports it.</param>
    /// <returns>The host to put in a URL.</returns>
    public static string UrlHost(string host)
    {
        return host.IndexOf(':') >= 0
            && !host.StartsWith("[", StringComparison.Ordinal)
            && IPAddress.TryParse(host, out var address)
            && address.AddressFamily == AddressFamily.InterNetworkV6
                ? "[" + host + "]"
                : host;
    }

    /// <summary>
    /// Builds an emulator base URL, <c>scheme://host:port</c> with no trailing slash, bracketing
    /// an IPv6 host.
    /// </summary>
    /// <param name="scheme">The URL scheme, <c>http</c> or <c>https</c>.</param>
    /// <param name="host">A host name or IP address.</param>
    /// <param name="port">The port.</param>
    /// <returns>The base URL, e.g. <c>http://localhost:32781</c> or <c>http://[::1]:32781</c>.</returns>
    public static string BaseUrl(string scheme, string host, int port)
    {
        return scheme + "://" + HostAndPort(host, port);
    }

    /// <summary>
    /// Builds an emulator address without a scheme, <c>host:port</c>, bracketing an IPv6 host.
    /// </summary>
    /// <param name="host">A host name or IP address.</param>
    /// <param name="port">The port.</param>
    /// <returns>The address, e.g. <c>localhost:32781</c> or <c>[::1]:32781</c>.</returns>
    public static string HostAndPort(string host, int port)
    {
        return UrlHost(host) + ":" + port.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Removes the sibling containers Floci spawned under <paramref name="resourceNamespace" />.
    /// Floci manages them itself and the Testcontainers resource reaper does not track them, so
    /// without this they outlive the emulator: a leaked ECR registry, for one, keeps its fixed host
    /// port and makes the next emulator's registry fail to start. Failures are ignored, since this
    /// runs during disposal.
    /// </summary>
    /// <param name="endpoint">The Docker endpoint the emulator ran on.</param>
    /// <param name="resourceNamespace">The emulator's resource namespace.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the siblings are removed.</returns>
    public static async Task RemoveSiblingContainersAsync(
        IDockerEndpointAuthenticationConfiguration endpoint,
        string resourceNamespace,
        CancellationToken ct = default)
    {
        try
        {
            using var client = endpoint.GetDockerClientBuilder(Guid.NewGuid()).Build();
            var siblings = await client.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    All = true,
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["label"] = new Dictionary<string, bool> { [NamespaceLabel + "=" + resourceNamespace] = true },
                    },
                },
                ct).ConfigureAwait(false);

            foreach (var sibling in siblings)
            {
                try
                {
                    await client.Containers.RemoveContainerAsync(
                        sibling.ID, new ContainerRemoveParameters { Force = true, RemoveVolumes = true }, ct).ConfigureAwait(false);
                }
                catch (DockerApiException)
                {
                    // Already gone, or Floci removed it concurrently.
                }
            }
        }
        catch (Exception exception) when (exception is DockerApiException or HttpRequestException or TimeoutException)
        {
            // Best effort: never fail a test's teardown over leftover siblings.
        }
    }

    /// <summary>
    /// Wipes all emulator state (buckets, queues, tables, ...) without restarting the container.
    /// </summary>
    /// <param name="cloud">The emulator's descriptor.</param>
    /// <param name="endpoint">The emulator's base URL.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the state is reset.</returns>
    public static async Task ResetAsync(CloudDescriptor cloud, string endpoint, CancellationToken ct = default)
    {
        if (cloud.ResetPath is null)
        {
            throw new NotSupportedException($"The {cloud.Name} emulator has no state reset.");
        }

        var uri = new Uri(new Uri(endpoint), cloud.ResetPath);
        using var response = await Http.PostAsync(uri, content: null, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"State reset failed with HTTP {(int)response.StatusCode}. The {cloud.Name} emulator image may predate "
                + $"{cloud.ResetPath} (floci/floci added it in 1.5.29).");
        }
    }
}
