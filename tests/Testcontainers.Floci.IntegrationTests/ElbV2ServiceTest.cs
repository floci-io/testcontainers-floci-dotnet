using System.Threading.Tasks;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ElbV2ServiceTest : IAsyncLifetime
{
    private const int ListenerPort = 8085;

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithElbV2(new ElbV2Config { ListenerPorts = new[] { ListenerPort } })
        .Build();

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

    [Fact]
    public void ListenerPortIsBoundOneToOneOnTheHost()
    {
        // Floci returns endpoint=localhost:<listenerPort> literally, so the port must be published
        // 1:1 (host == container) rather than mapped to a random host port. If this fails with a
        // different host port, the FixedHostPorts wiring is wrong and ListenerPorts should instead
        // drive randomly-mapped exposed ports (matching the upstream Java behaviour).
        Assert.Equal(ListenerPort, _floci.GetMappedPublicPort(ListenerPort));
    }
}
