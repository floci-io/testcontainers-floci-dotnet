using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.ECS;
using Amazon.ECS.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EcsServiceTest : IAsyncLifetime
{
    private const string ClusterName = "test-cluster";
    private const string TaskFamily = "test-task";

    // Mock mode: ECS tasks go straight to RUNNING without spawning real Docker containers.
    // This makes the test deterministic — no container pull, no startup delay, no leaks.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEcs(new EcsConfig { Mock = true })
        .Build();

    private string? _taskArn;

    public System.Threading.Tasks.Task InitializeAsync() => _floci.StartAsync();

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        // Best-effort cleanup: stop the task and delete the cluster. ECS state is in-memory
        // (mock spawns nothing), so leaks are not a concern, but tidiness matters for re-runs.
        try
        {
            using var ecs = CreateClient();

            if (_taskArn != null)
            {
                await ecs.StopTaskAsync(new StopTaskRequest
                {
                    Cluster = ClusterName,
                    Task = _taskArn,
                });
            }

            await ecs.DeleteClusterAsync(new DeleteClusterRequest { Cluster = ClusterName });
        }
        catch (AmazonECSException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonECSClient CreateClient()
    {
        return new AmazonECSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonECSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async System.Threading.Tasks.Task CreatesClusterRegistersTaskDefinitionAndRunsTask()
    {
        using var ecs = CreateClient();

        // Create cluster
        var clusterResponse = await ecs.CreateClusterAsync(
            new CreateClusterRequest { ClusterName = ClusterName });

        Assert.NotEmpty(clusterResponse.Cluster.ClusterArn);

        // Verify the cluster appears in ListClusters
        var listResponse = await ecs.ListClustersAsync(new ListClustersRequest());
        Assert.Contains(clusterResponse.Cluster.ClusterArn, listResponse.ClusterArns);

        // Register a task definition
        await ecs.RegisterTaskDefinitionAsync(new RegisterTaskDefinitionRequest
        {
            Family = TaskFamily,
            ContainerDefinitions = new List<ContainerDefinition>
            {
                new ContainerDefinition
                {
                    Name = "app",
                    Image = "public.ecr.aws/docker/library/alpine:3.20",
                    Essential = true,
                    Command = new List<string> { "sh", "-c", "sleep 30" },
                    Memory = 128,
                },
            },
        });

        // Run the task — mock mode returns RUNNING immediately, no container is started
        var runResponse = await ecs.RunTaskAsync(new RunTaskRequest
        {
            Cluster = ClusterName,
            TaskDefinition = TaskFamily,
        });

        Assert.Single(runResponse.Tasks);
        _taskArn = runResponse.Tasks[0].TaskArn;
        Assert.Equal("RUNNING", runResponse.Tasks[0].LastStatus);
    }
}
