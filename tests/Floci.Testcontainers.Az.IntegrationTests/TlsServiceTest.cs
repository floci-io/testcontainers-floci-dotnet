using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class TlsServiceTest
{
    [Fact]
    public async Task ServesHttpsWithTheCertificateItHandsOut()
    {
        await using var floci = new FlociAzBuilder(TestImages.FlociAz).WithTls().Build();
        await floci.StartAsync();

        var pem = await floci.GetTlsCertificateAsync();
        Assert.Contains("BEGIN CERTIFICATE", pem);

        // Trust exactly the handed-out certificate, as an SDK client would after importing it.
        using var anchor = X509Certificate2.CreateFromPem(pem);
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
            {
                if (certificate is null)
                {
                    return false;
                }

                using var chain = new X509Chain();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(anchor);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                return chain.Build(certificate);
            },
        };

        // Straight after start, with HttpClient's defaults: the container waited for HTTPS itself.
        using var http = new HttpClient(handler);
        using var response = await http.GetAsync(floci.GetHttpsEndpoint() + AzCloud.Descriptor.HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
