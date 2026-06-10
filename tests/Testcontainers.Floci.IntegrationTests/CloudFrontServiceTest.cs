using System.Threading.Tasks;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudFrontServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCloudFrontClient CreateClient()
    {
        return new AmazonCloudFrontClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCloudFrontConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsOriginAccessIdentity()
    {
        using var cf = CreateClient();
        const string comment = "test-oai";

        var created = await cf.CreateCloudFrontOriginAccessIdentityAsync(
            new CreateCloudFrontOriginAccessIdentityRequest
            {
                CloudFrontOriginAccessIdentityConfig = new CloudFrontOriginAccessIdentityConfig
                {
                    CallerReference = "test-ref",
                    Comment = comment,
                },
            });

        var list = await cf.ListCloudFrontOriginAccessIdentitiesAsync(
            new ListCloudFrontOriginAccessIdentitiesRequest());

        Assert.Contains(list.CloudFrontOriginAccessIdentityList.Items,
            i => i.Id == created.CloudFrontOriginAccessIdentity.Id);
    }
}
