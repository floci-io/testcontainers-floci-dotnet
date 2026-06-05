using System;
using System.Threading.Tasks;
using Amazon.ElastiCache;
using Amazon.ElastiCache.Model;
using StackExchange.Redis;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ElastiCacheServiceTest : IAsyncLifetime
{
    private const string ReplicationGroupId = "test-rg";

    // Base port 6390 avoids the default 6379, which may be occupied by a local Redis/Valkey
    // instance on developer machines. Ports are published 1:1 so the Floci proxy is reachable
    // on the host at 127.0.0.1:6390.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithElastiCache(new ElastiCacheConfig { ProxyBasePort = 6390 })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the replication group so Floci tears down the sibling Valkey container it
        // spawned. Those siblings are Floci-managed (not Testcontainers/Ryuk-tracked), so
        // without this they leak — and since they're named after the group id, a leak would
        // collide on the next run. Best-effort: the Floci container is torn down regardless.
        try
        {
            using var elastiCache = CreateClient();
            await elastiCache.DeleteReplicationGroupAsync(new DeleteReplicationGroupRequest
            {
                ReplicationGroupId = ReplicationGroupId,
                RetainPrimaryCluster = false,
            });
        }
        catch (AmazonElastiCacheException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonElastiCacheClient CreateClient()
    {
        return new AmazonElastiCacheClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonElastiCacheConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesRedisReplicationGroupAndConnects()
    {
        using var elastiCache = CreateClient();

        var createResponse = await elastiCache.CreateReplicationGroupAsync(
            new CreateReplicationGroupRequest
            {
                ReplicationGroupId = ReplicationGroupId,
                ReplicationGroupDescription = "test",
                Engine = "redis",
                CacheNodeType = "cache.t3.micro",
                NumCacheClusters = 1,
            });

        Assert.Equal("available", createResponse.ReplicationGroup.Status);

        // Floci publishes the proxy as localhost:<port>, with the port published 1:1 to the host.
        // Use 127.0.0.1 explicitly to avoid StackExchange.Redis resolving localhost to IPv6 (::1).
        var redisPort = _floci.GetMappedPublicPort(6390);
        var connectionString = $"127.0.0.1:{redisPort},abortConnect=false,connectTimeout=3000";

        // The Valkey sibling starts asynchronously after the API call returns; retry until the
        // round-trip succeeds (typically ~2 s).
        Exception? lastError = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var mux = ConnectionMultiplexer.Connect(connectionString);
                var db = mux.GetDatabase();
                db.StringSet("hello", "world");
                var value = (string?)db.StringGet("hello");
                Assert.Equal("world", value);
                return;
            }
            catch (Exception ex) when (ex is RedisConnectionException or RedisException or System.Net.Sockets.SocketException)
            {
                lastError = ex;
                await Task.Delay(1000);
            }
        }

        throw new Xunit.Sdk.XunitException(
            $"Could not connect to ElastiCache proxy at 127.0.0.1:{redisPort} after retries. " +
            $"Last error: {lastError?.Message}");
    }
}
