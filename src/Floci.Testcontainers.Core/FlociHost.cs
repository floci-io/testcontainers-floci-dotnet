using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
