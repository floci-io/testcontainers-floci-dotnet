using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Amazon.Neptune;
using Amazon.Neptune.Model;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class NeptuneServiceTest : IAsyncLifetime
{
    private const string ClusterId = "test-neptune";
    private const int ProxyPort = 8182;

    // ProxyPortsCount=1 publishes only port 8182. The upstream default (101) would bind 101 host
    // ports 1:1 on the gateway, which is needless for a single-cluster test.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithNeptune(new NeptuneConfig { ProxyBasePort = ProxyPort, ProxyPortsCount = 1 })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the cluster so Floci tears down the sibling Gremlin Server container it spawned.
        try
        {
            using var neptune = CreateClient();
            await neptune.DeleteDBClusterAsync(new DeleteDBClusterRequest
            {
                DBClusterIdentifier = ClusterId,
                SkipFinalSnapshot = true,
            });
        }
        catch (AmazonNeptuneException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonNeptuneClient CreateClient()
    {
        return new AmazonNeptuneClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonNeptuneConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                // CreateDBCluster can block while Floci pulls the Gremlin Server image on first use.
                Timeout = TimeSpan.FromMinutes(5),
            });
    }

    [Fact]
    public async Task CreatesClusterAndRunsGremlinQuery()
    {
        using var neptune = CreateClient();

        // Control plane: create the cluster and confirm it via describe.
        await neptune.CreateDBClusterAsync(new CreateDBClusterRequest
        {
            DBClusterIdentifier = ClusterId,
            Engine = "neptune",
        });

        var described = await neptune.DescribeDBClustersAsync(new DescribeDBClustersRequest
        {
            DBClusterIdentifier = ClusterId,
        });
        var cluster = described.DBClusters[0];
        Assert.Equal(ClusterId, cluster.DBClusterIdentifier);
        Assert.Equal("neptune", cluster.Engine);
        Assert.Equal(ProxyPort, cluster.Port);

        // Data plane: connect a Gremlin client through the gateway proxy and run a real graph
        // round-trip — beyond the upstream Java test, which only exercises the control plane.
        var count = await AddVertexAndCountWithRetryAsync();

        Assert.True(count >= 1, $"expected at least one vertex, got {count}");
    }

    private static async Task<long> AddVertexAndCountWithRetryAsync()
    {
        Exception? lastError = null;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                // Floci returns the endpoint as "localhost"; connect via 127.0.0.1 to avoid the
                // client resolving to IPv6 (::1), which Testcontainers does not publish.
                var server = new GremlinServer("127.0.0.1", ProxyPort);
                using var client = new GremlinClient(server);

                await client.SubmitAsync<object>("g.addV('person').property('name', 'floci')");
                return await client.SubmitWithSingleResultAsync<long>(
                    "g.V().hasLabel('person').count()");
            }
            catch (Exception ex) when (
                ex is ResponseException or WebSocketException or SocketException
                    or TimeoutException or ConnectionClosedException)
            {
                // The Gremlin Server container needs a few seconds to accept connections.
                lastError = ex;
                await Task.Delay(2000);
            }
        }

        throw new Xunit.Sdk.XunitException(
            $"Gremlin endpoint did not become usable within the timeout. Last error: {lastError?.Message}");
    }
}
