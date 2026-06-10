using System.Net;
using System.Threading.Tasks;
using Amazon.ResourceGroupsTaggingAPI;
using Amazon.ResourceGroupsTaggingAPI.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ResourceGroupsTaggingServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithResourceGroupsTagging(new ResourceGroupsTaggingConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonResourceGroupsTaggingAPIClient CreateClient()
    {
        return new AmazonResourceGroupsTaggingAPIClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonResourceGroupsTaggingAPIConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task GetResourcesSucceeds()
    {
        using var tagging = CreateClient();

        var response = await tagging.GetResourcesAsync(new GetResourcesRequest());

        Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
    }
}
