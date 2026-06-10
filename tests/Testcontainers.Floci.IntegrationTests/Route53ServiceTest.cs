using System.Threading.Tasks;
using Amazon.Route53;
using Amazon.Route53.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class Route53ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonRoute53Client CreateClient()
    {
        return new AmazonRoute53Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonRoute53Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsHostedZone()
    {
        using var route53 = CreateClient();
        const string domainName = "example.com.";

        await route53.CreateHostedZoneAsync(new CreateHostedZoneRequest
        {
            Name = domainName,
            CallerReference = "test-ref-1",
        });

        var zones = await route53.ListHostedZonesAsync(new ListHostedZonesRequest());

        Assert.Contains(zones.HostedZones, z => z.Name == domainName);
    }
}
