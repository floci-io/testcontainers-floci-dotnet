using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.EKS;
using Amazon.EKS.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EksNodeGroupServiceTest : IAsyncLifetime
{
    private const string ClusterName = "eks-ng-test";
    private const string NodegroupName = "ng-1";

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEks(new EksConfig { Mock = true })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Best-effort cluster delete; Floci removes any mock-mode resources it tracks.
        try
        {
            using var eks = CreateClient();
            await eks.DeleteClusterAsync(new DeleteClusterRequest { Name = ClusterName });
        }
        catch (AmazonEKSException)
        {
            // Container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonEKSClient CreateClient()
    {
        return new AmazonEKSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonEKSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesClusterAndManagedNodeGroup()
    {
        using var eks = CreateClient();

        // Step 1: create the cluster.
        await eks.CreateClusterAsync(new CreateClusterRequest
        {
            Name = ClusterName,
            RoleArn = "arn:aws:iam::000000000000:role/eks-role",
            ResourcesVpcConfig = new VpcConfigRequest(),
        });

        // Step 2: poll until the cluster is ACTIVE (mock mode is immediate, but loop for safety).
        await WaitForActiveClusterAsync(eks);

        // Step 3: create the managed node group.
        await eks.CreateNodegroupAsync(new CreateNodegroupRequest
        {
            ClusterName = ClusterName,
            NodegroupName = NodegroupName,
            NodeRole = "arn:aws:iam::000000000000:role/eks-node-role",
            Subnets = new List<string> { "subnet-12345678" },
        });

        // Step 4: poll until the node group is ACTIVE.
        await WaitForActiveNodegroupAsync(eks);

        // Step 5: list node groups and assert "ng-1" is present.
        var listResp = await eks.ListNodegroupsAsync(new ListNodegroupsRequest { ClusterName = ClusterName });
        Assert.Contains(NodegroupName, listResp.Nodegroups);

        // Step 6: delete the node group.
        var deleteResp = await eks.DeleteNodegroupAsync(new DeleteNodegroupRequest
        {
            ClusterName = ClusterName,
            NodegroupName = NodegroupName,
        });
        Assert.NotNull(deleteResp.Nodegroup);
    }

    private async Task WaitForActiveClusterAsync(AmazonEKSClient eks)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var cluster = (await eks.DescribeClusterAsync(new DescribeClusterRequest { Name = ClusterName })).Cluster;
            if (cluster.Status == ClusterStatus.ACTIVE)
            {
                return;
            }

            Assert.NotEqual(ClusterStatus.FAILED, cluster.Status);
            await Task.Delay(500);
        }

        throw new Xunit.Sdk.XunitException("EKS cluster did not become ACTIVE within the timeout.");
    }

    private async Task WaitForActiveNodegroupAsync(AmazonEKSClient eks)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var ng = (await eks.DescribeNodegroupAsync(new DescribeNodegroupRequest
            {
                ClusterName = ClusterName,
                NodegroupName = NodegroupName,
            })).Nodegroup;

            if (ng.Status == NodegroupStatus.ACTIVE)
            {
                return;
            }

            Assert.NotEqual(NodegroupStatus.CREATE_FAILED, ng.Status);
            await Task.Delay(500);
        }

        throw new Xunit.Sdk.XunitException("EKS node group did not become ACTIVE within the timeout.");
    }
}
