using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.ElasticMapReduce;
using Amazon.ElasticMapReduce.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EmrServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEmr(new EmrConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonElasticMapReduceClient CreateClient()
    {
        return new AmazonElasticMapReduceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonElasticMapReduceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                Timeout = TimeSpan.FromSeconds(30),
            });
    }

    [Fact]
    public async Task CreatesAndListsCluster()
    {
        using var emr = CreateClient();

        string jobFlowId;
        try
        {
            var runResponse = await emr.RunJobFlowAsync(new RunJobFlowRequest
            {
                Name = "test-cluster",
                ReleaseLabel = "emr-7.5.0",
                Instances = new JobFlowInstancesConfig
                {
                    InstanceCount = 1,
                    MasterInstanceType = "m5.xlarge",
                    KeepJobFlowAliveWhenNoSteps = true,
                },
                ServiceRole = "EMR_DefaultRole",
                JobFlowRole = "EMR_EC2_DefaultRole",
            });

            jobFlowId = runResponse.JobFlowId;
            Assert.False(string.IsNullOrEmpty(jobFlowId), "RunJobFlow must return a non-empty cluster id.");
        }
        catch (Exception)
        {
            // RunJobFlow failed — re-throw so the test fails with full detail.
            throw;
        }

        try
        {
            var describeResponse = await emr.DescribeClusterAsync(new DescribeClusterRequest
            {
                ClusterId = jobFlowId,
            });

            Assert.NotNull(describeResponse.Cluster);
            Assert.Equal(jobFlowId, describeResponse.Cluster.Id);

            var listResponse = await emr.ListClustersAsync(new ListClustersRequest());
            Assert.Contains(listResponse.Clusters, c => c.Id == jobFlowId);
        }
        finally
        {
            try
            {
                await emr.TerminateJobFlowsAsync(new TerminateJobFlowsRequest
                {
                    JobFlowIds = new System.Collections.Generic.List<string> { jobFlowId },
                });
            }
            catch
            {
                // Best-effort teardown — do not mask the primary assertion failure.
            }
        }
    }
}
