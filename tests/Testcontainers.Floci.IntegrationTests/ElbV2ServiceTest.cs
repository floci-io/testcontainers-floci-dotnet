using System.Threading.Tasks;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ElbV2ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonElasticLoadBalancingV2Client CreateClient()
    {
        return new AmazonElasticLoadBalancingV2Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonElasticLoadBalancingV2Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task DescribesLoadBalancers()
    {
        using var elb = CreateClient();

        var response = await elb.DescribeLoadBalancersAsync(new DescribeLoadBalancersRequest());

        Assert.NotNull(response.LoadBalancers);
    }
}
