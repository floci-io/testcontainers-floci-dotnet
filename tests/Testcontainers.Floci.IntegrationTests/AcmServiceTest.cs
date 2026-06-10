using System.Threading.Tasks;
using Amazon.CertificateManager;
using Amazon.CertificateManager.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AcmServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCertificateManagerClient CreateClient()
    {
        return new AmazonCertificateManagerClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCertificateManagerConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task RequestsAndListsCertificate()
    {
        using var acm = CreateClient();

        var request = await acm.RequestCertificateAsync(new RequestCertificateRequest
        {
            DomainName = "example.com",
            ValidationMethod = ValidationMethod.DNS,
        });

        var certs = await acm.ListCertificatesAsync(new ListCertificatesRequest());

        Assert.Contains(certs.CertificateSummaryList, c => c.CertificateArn == request.CertificateArn);
    }
}
