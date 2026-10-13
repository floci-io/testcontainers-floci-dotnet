using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class CoreServiceTest : IClassFixture<FlociAzFixture>
{
    private readonly FlociAzContainer _floci;

    public CoreServiceTest(FlociAzFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task StartsHealthy()
    {
        using var http = new HttpClient();
        using var response = await http.GetAsync(_floci.GetEndpoint() + AzCloud.Descriptor.HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AzCloud.DefaultSubscriptionId, _floci.SubscriptionId);
        Assert.Equal(AzCloud.DefaultTenantId, _floci.TenantId);
    }

    [Fact]
    public async Task ResetWipesStateWithoutRestarting()
    {
        var blobs = new BlobServiceClient(_floci.GetStorageConnectionString());
        await blobs.CreateBlobContainerAsync("reset-me");
        Assert.Contains(await ContainerNamesAsync(blobs), name => name == "reset-me");

        await _floci.ResetAsync();

        Assert.DoesNotContain(await ContainerNamesAsync(blobs), name => name == "reset-me");
    }

    [Fact]
    public async Task HasAUniqueResourceNamespacePerStart()
    {
        await using var other = new FlociAzBuilder(TestImages.FlociAz).Build();
        await other.StartAsync();

        Assert.Matches("^tc-[0-9a-f]{8}$", _floci.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", other.ResourceNamespace);
        Assert.NotEqual(_floci.ResourceNamespace, other.ResourceNamespace);
    }

    private static async Task<string[]> ContainerNamesAsync(BlobServiceClient blobs)
    {
        var names = new System.Collections.Generic.List<string>();
        await foreach (var container in blobs.GetBlobContainersAsync())
        {
            names.Add(container.Name);
        }

        return names.ToArray();
    }
}
