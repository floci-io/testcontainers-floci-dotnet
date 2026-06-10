using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Kafka;
using Amazon.Kafka.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class MskServiceTest : IAsyncLifetime
{
    // Mock mode keeps the test deterministic and avoids pulling the Redpanda broker image: the
    // control plane (create/list clusters) is simulated in-memory without spawning a container.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithMsk(new MskConfig { Mock = true })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonKafkaClient CreateClient()
    {
        return new AmazonKafkaClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonKafkaConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsCluster()
    {
        using var kafka = CreateClient();
        const string clusterName = "test-cluster";

        var created = await kafka.CreateClusterAsync(new CreateClusterRequest
        {
            ClusterName = clusterName,
            KafkaVersion = "3.6.0",
            NumberOfBrokerNodes = 1,
            BrokerNodeGroupInfo = new BrokerNodeGroupInfo
            {
                InstanceType = "kafka.m5.large",
                ClientSubnets = new List<string> { "subnet-12345" },
            },
        });

        Assert.False(string.IsNullOrEmpty(created.ClusterArn));

        var clusters = await kafka.ListClustersAsync(new ListClustersRequest());

        Assert.Contains(clusters.ClusterInfoList, c => c.ClusterName == clusterName);
    }
}
