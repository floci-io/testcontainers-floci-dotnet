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
    public async Task CreatesAndGetsDistribution()
    {
        using var cf = CreateClient();
        const string callerReference = "test-distribution-ref";

        var created = await cf.CreateDistributionAsync(new CreateDistributionRequest
        {
            DistributionConfig = new DistributionConfig
            {
                CallerReference = callerReference,
                Comment = "Test distribution",
                Enabled = true,
                Origins = new Origins
                {
                    Quantity = 1,
                    Items = new System.Collections.Generic.List<Origin>
                    {
                        new Origin
                        {
                            Id = "test-origin",
                            DomainName = "example.com",
                            CustomOriginConfig = new CustomOriginConfig
                            {
                                HTTPPort = 80,
                                HTTPSPort = 443,
                                OriginProtocolPolicy = OriginProtocolPolicy.HttpOnly,
                            },
                        },
                    },
                },
                // Floci emulates the legacy CloudFront API, which requires MinTTL + ForwardedValues
                // rather than a CachePolicyId (mirrors the upstream Java test).
#pragma warning disable CS0618
                DefaultCacheBehavior = new DefaultCacheBehavior
                {
                    TargetOriginId = "test-origin",
                    ViewerProtocolPolicy = ViewerProtocolPolicy.AllowAll,
                    MinTTL = 0L,
                    ForwardedValues = new ForwardedValues
                    {
                        QueryString = false,
                        Cookies = new CookiePreference { Forward = ItemSelection.None },
                    },
                    TrustedSigners = new TrustedSigners { Enabled = false, Quantity = 0 },
                },
#pragma warning restore CS0618
            },
        });

        Assert.False(string.IsNullOrEmpty(created.Distribution.Id));

        var got = await cf.GetDistributionAsync(new GetDistributionRequest { Id = created.Distribution.Id });

        Assert.Equal(created.Distribution.Id, got.Distribution.Id);
        Assert.Equal(callerReference, got.Distribution.DistributionConfig.CallerReference);
    }
}
