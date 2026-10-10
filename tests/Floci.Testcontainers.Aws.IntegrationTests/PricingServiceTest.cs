using System.Threading.Tasks;
using Amazon.Pricing;
using Amazon.Pricing.Model;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class PricingServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonPricingClient CreateClient()
    {
        return new AmazonPricingClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonPricingConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task DescribesServices()
    {
        using var pricing = CreateClient();

        var response = await pricing.DescribeServicesAsync(new DescribeServicesRequest());

        Assert.NotNull(response.Services);
    }
}
