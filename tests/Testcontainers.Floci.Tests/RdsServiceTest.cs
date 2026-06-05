using System;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using Npgsql;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class RdsServiceTest : IAsyncLifetime
{
    private const string MasterUsername = "testuser";
    private const string MasterUserPassword = "testpassword123";
    private const string InstanceId = "test-instance";

    // Base port 7010 avoids the default 7000, which collides with macOS Control Center
    // (AirPlay Receiver) — that listens on *:7000 and would intercept host connections.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithRds(new RdsConfig { ProxyBasePort = 7010 })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the instance so Floci tears down the sibling database container it spawned.
        // Those siblings are Floci-managed (not Testcontainers/Ryuk-tracked), so without this
        // they leak — and since they're named after the instance id, a leak would collide on
        // the next run. Best-effort: the container is torn down regardless immediately after.
        try
        {
            using var rds = CreateClient();
            await rds.DeleteDBInstanceAsync(new DeleteDBInstanceRequest
            {
                DBInstanceIdentifier = InstanceId,
                SkipFinalSnapshot = true,
            });
        }
        catch (AmazonRDSException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonRDSClient CreateClient()
    {
        return new AmazonRDSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonRDSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesPostgresInstanceAndConnects()
    {
        using var rds = CreateClient();

        await rds.CreateDBInstanceAsync(new CreateDBInstanceRequest
        {
            DBInstanceIdentifier = InstanceId,
            Engine = "postgres",
            DBInstanceClass = "db.t3.micro",
            MasterUsername = MasterUsername,
            MasterUserPassword = MasterUserPassword,
            AllocatedStorage = 20,
        });

        var endpoint = await WaitForEndpointAsync(rds, InstanceId);

        // Floci returns the proxy as "localhost"; the proxy port is published on the host
        // loopback. Use 127.0.0.1 explicitly to avoid Npgsql resolving localhost to IPv6 (::1),
        // which Testcontainers does not publish to.
        var host = endpoint.Address is "localhost" or "127.0.0.1" ? "127.0.0.1" : endpoint.Address;
        var connectionString =
            $"Host={host};Port={endpoint.Port};Username={MasterUsername};" +
            $"Password={MasterUserPassword};Database=postgres;Timeout=3;SSL Mode=Disable";

        var result = await ConnectAndQueryWithRetryAsync(connectionString, endpoint);

        Assert.Equal(1, Convert.ToInt32(result));
    }

    // The backing Postgres container needs a few seconds to start accepting connections.
    private static async Task<object?> ConnectAndQueryWithRetryAsync(string connectionString, Endpoint endpoint)
    {
        NpgsqlException? lastError = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand("SELECT 1", connection);
                return await command.ExecuteScalarAsync();
            }
            catch (NpgsqlException ex)
            {
                lastError = ex;
                await Task.Delay(1000);
            }
        }

        throw new Xunit.Sdk.XunitException(
            $"Could not connect to RDS endpoint {endpoint.Address}:{endpoint.Port} after retries. " +
            $"Last error: {lastError?.Message}");
    }

    // Floci returns the proxy endpoint (e.g. localhost:7001) once the instance is provisioned.
    private static async Task<Endpoint> WaitForEndpointAsync(AmazonRDSClient rds, string instanceId)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var response = await rds.DescribeDBInstancesAsync(
                new DescribeDBInstancesRequest { DBInstanceIdentifier = instanceId });

            var endpoint = response.DBInstances[0].Endpoint;
            if (endpoint != null && !string.IsNullOrEmpty(endpoint.Address))
            {
                return endpoint;
            }

            await Task.Delay(1000);
        }

        throw new Xunit.Sdk.XunitException("RDS instance did not expose an endpoint within the timeout.");
    }
}
