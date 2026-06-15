using System;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using Amazon.RDSDataService;
using Amazon.RDSDataService.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

// NOTE: ExecuteStatement is skipped due to a Floci 1.5.25 native-image bug.
//
// Floci's RDS Data API implementation (floci/floci#1262) uses DriverManager.getConnection
// with jdbc:mysql:// URLs. The published Docker Hub image is a GraalVM native binary, and the
// MySQL JDBC driver (mysql-connector-j) is not registered in the native-image resource
// configuration (META-INF/services/java.sql.Driver is absent from resource-config.json).
// This causes every ExecuteStatement call against a MySQL/MariaDB cluster to fail with
// "No suitable driver found for jdbc:mysql://...".
//
// Floci's own compatibility tests run against a JVM build (docker/Dockerfile.jvm-package,
// using `java -jar quarkus-app/quarkus-run.jar`), where ServiceLoader auto-discovery works.
// The native build published to Docker Hub is missing this registration.
//
// Re-enable the [Fact] and remove the [Fact(Skip=...)] once this is fixed upstream.
// See: https://github.com/hectorvent/floci/blob/main/src/main/resources/META-INF/native-image/resource-config.json
// Fix would add: {"pattern":"META-INF/services/java.sql.Driver"} to the resources.includes array.
public sealed class RdsDataServiceTest : IAsyncLifetime
{
    private const string ClusterId = "rds-data-test-cluster";
    private const string MasterUsername = "testuser";
    private const string MasterUserPassword = "testpassword123";
    private const string DatabaseName = "app";
    private const string SecretName = "rds-data/test-creds";

    // RDS Data API requires a MySQL/MariaDB cluster (container-backed), so RDS needs the Docker
    // socket mounted and proxy ports published. Use base port 7030 to avoid conflicts with the
    // RDS integration test (7010) and macOS AirPlay Receiver (7000).
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithRds(new RdsConfig { ProxyBasePort = 7030 })
        .WithRdsData(new RdsDataConfig())
        .Build();

    private string? _clusterArn;
    private string? _secretArn;

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the cluster and secret so Floci tears down the sibling MySQL container.
        // These are Floci-managed (not Ryuk-tracked), so without cleanup they would leak.
        if (_clusterArn != null)
        {
            try
            {
                using var rds = CreateRdsClient();
                await rds.DeleteDBClusterAsync(new DeleteDBClusterRequest
                {
                    DBClusterIdentifier = ClusterId,
                    SkipFinalSnapshot = true,
                });
            }
            catch (AmazonRDSException)
            {
            }
        }

        if (_secretArn != null)
        {
            try
            {
                using var sm = CreateSecretsManagerClient();
                await sm.DeleteSecretAsync(new DeleteSecretRequest
                {
                    SecretId = _secretArn,
                    ForceDeleteWithoutRecovery = true,
                });
            }
            catch (AmazonSecretsManagerException)
            {
            }
        }

        await _floci.DisposeAsync();
    }

    private AmazonRDSClient CreateRdsClient()
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

    private AmazonSecretsManagerClient CreateSecretsManagerClient()
    {
        return new AmazonSecretsManagerClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSecretsManagerConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    private AmazonRDSDataServiceClient CreateRdsDataClient()
    {
        return new AmazonRDSDataServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonRDSDataServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                // Allow enough time for the MySQL container to be ready on first invoke.
                Timeout = TimeSpan.FromSeconds(120),
            });
    }

    [Fact(Skip = "Floci 1.5.25 native-image bug: mysql-connector-j not registered for native JDBC access — see comment above. Remove Skip when upstream fixes resource-config.json.")]
    public async Task ExecutesStatementAgainstMysqlCluster()
    {
        // Step 1: Create an aurora-mysql cluster. Floci spawns a mysql:8.0 sibling container
        // and returns status=available immediately (provisioning is synchronous in Floci).
        using var rds = CreateRdsClient();
        var createCluster = await rds.CreateDBClusterAsync(new CreateDBClusterRequest
        {
            DBClusterIdentifier = ClusterId,
            Engine = "aurora-mysql",
            MasterUsername = MasterUsername,
            MasterUserPassword = MasterUserPassword,
            DatabaseName = DatabaseName,
        });
        _clusterArn = createCluster.DBCluster.DBClusterArn;

        // Step 2: Create a Secrets Manager secret with the cluster credentials in the format
        // Floci's RDS Data API resolver expects: {"username":"...","password":"..."}.
        using var sm = CreateSecretsManagerClient();
        var createSecret = await sm.CreateSecretAsync(new CreateSecretRequest
        {
            Name = SecretName,
            SecretString = $"{{\"username\":\"{MasterUsername}\",\"password\":\"{MasterUserPassword}\"}}",
        });
        _secretArn = createSecret.ARN;

        // Step 3: Execute a statement. The MySQL container may still be initialising, so retry
        // for up to 90 seconds (matching Floci's own compatibility-test retry window).
        using var data = CreateRdsDataClient();
        ExecuteStatementResponse? result = null;
        Exception? lastError = null;

        for (var attempt = 0; attempt < 90; attempt++)
        {
            try
            {
                result = await data.ExecuteStatementAsync(new ExecuteStatementRequest
                {
                    ResourceArn = _clusterArn,
                    SecretArn = _secretArn,
                    Database = DatabaseName,
                    Sql = "SELECT 1",
                });
                break;
            }
            catch (Exception ex)
            {
                lastError = ex;
                await Task.Delay(1000);
            }
        }

        if (result == null)
        {
            throw new Xunit.Sdk.XunitException(
                $"ExecuteStatement did not succeed within the retry window. Last error: {lastError?.Message}");
        }

        Assert.NotNull(result.Records);
        Assert.Single(result.Records);
        var row = result.Records[0];
        Assert.Single(row);
        Assert.Equal(1L, row[0].LongValue);
    }
}
