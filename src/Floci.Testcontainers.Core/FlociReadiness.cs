using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Core;

/// <summary>
/// Readiness shared by the Floci cloud packages, beyond each module's plain-HTTP health wait.
/// </summary>
[PublicAPI]
public static class FlociReadiness
{
    /// <summary>
    /// Returns <paramref name="current" />, followed by a wait for the health path over HTTPS when
    /// TLS is enabled in <paramref name="env" /> (see <see cref="CloudDescriptor.TlsEnabled" />).
    /// </summary>
    /// <remarks>
    /// With TLS enabled an emulator answers HTTP and HTTPS on the same port through a proxy that
    /// looks at the first byte of each connection. The proxy accepts connections, and plain HTTP
    /// is healthy, a few hundred milliseconds before the HTTPS backend listens; a TLS handshake in
    /// that window is closed mid-way. Waiting for one HTTPS request to succeed closes the window.
    /// The probe does not validate the certificate, which is self-signed by default.
    /// </remarks>
    /// <param name="cloud">The emulator's descriptor.</param>
    /// <param name="env">The container environment, as it will be when the container starts.</param>
    /// <param name="current">The wait strategies configured so far.</param>
    /// <returns>The wait strategies to start the container with.</returns>
    public static IEnumerable<WaitStrategy> WithHttpsWhenTlsEnabled(
        CloudDescriptor cloud,
        IReadOnlyDictionary<string, string> env,
        IEnumerable<WaitStrategy> current)
    {
        if (!cloud.TlsEnabled(env))
        {
            return current;
        }

        // Not disposed: the handler lives as long as the wait strategy that holds it.
        var anyCertificate = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        var https = Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPath(cloud.HealthPath)
                .ForPort(cloud.Port)
                .UsingTls()
                .UsingHttpMessageHandler(anyCertificate))
            .Build();

        return current.Concat(https).ToArray();
    }
}
