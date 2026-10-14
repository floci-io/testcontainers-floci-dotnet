using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Floci.Testcontainers.Oci;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

public sealed class TlsServiceTest
{
    [Fact]
    public async Task ServesHttpsAsSoonAsItHasStarted()
    {
        // The module has no TLS helpers yet (the emulator does not hand out its certificate), but
        // TLS can be switched on by its setting, and the container then waits for HTTPS as well.
        await using var floci = new FlociOciBuilder(TestImages.FlociOci)
            .WithEnvironment(OciCloud.Descriptor.TlsEnabledEnv, "true")
            .Build();
        await floci.StartAsync();

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
        using var http = new HttpClient(handler);
        var endpoint = floci.GetEndpoint();

        // Straight after start, with no retry.
        using var https = await http.GetAsync("https" + endpoint.Substring("http".Length) + OciCloud.Descriptor.HealthPath);
        Assert.Equal(HttpStatusCode.OK, https.StatusCode);

        // Plain HTTP stays available on the same port.
        using var plain = await http.GetAsync(endpoint + OciCloud.Descriptor.HealthPath);
        Assert.Equal(HttpStatusCode.OK, plain.StatusCode);
    }
}
