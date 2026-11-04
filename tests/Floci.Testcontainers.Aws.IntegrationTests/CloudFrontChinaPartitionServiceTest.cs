using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class CloudFrontChinaPartitionServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithRegion("cn-north-1")
        .WithCloudFront(new CloudFrontConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    [Fact]
    public async Task DefaultConfigAssignsTheChinaSuffix()
    {
        using var cf = new AmazonCloudFrontClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCloudFrontConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });

        var created = await cf.CreateDistributionAsync(new CreateDistributionRequest
        {
            DistributionConfig = new DistributionConfig
            {
                CallerReference = "china-distribution-ref",
                Comment = "China distribution",
                Enabled = true,
                Origins = new Origins
                {
                    Quantity = 1,
                    Items = new List<Origin>
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

        Assert.EndsWith(".cloudfront.cn", created.Distribution.DomainName);
    }
}
