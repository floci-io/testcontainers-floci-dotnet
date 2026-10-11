using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.ECR;
using Amazon.ECR.Model;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class SiblingCleanupServiceTest
{
    [Fact]
    public async Task DisposeRemovesTheSiblingContainersFlociSpawned()
    {
        // await using: a failed assertion still disposes the emulator and its siblings.
        await using var floci = new FlociBuilder(TestImages.Floci).WithEcr(new EcrConfig()).Build();
        await floci.StartAsync();
        var resourceNamespace = floci.ResourceNamespace!;

        // Creating a repository makes Floci start its ECR registry as a sibling container.
        using (var ecr = new AmazonECRClient(
                   floci.AccessKey,
                   floci.SecretKey,
                   new AmazonECRConfig { ServiceURL = floci.GetEndpoint(), AuthenticationRegion = floci.Region }))
        {
            await ecr.CreateRepositoryAsync(new CreateRepositoryRequest { RepositoryName = "cleanup-test" });
        }

        Assert.NotEmpty(await SiblingsAsync(resourceNamespace));

        await floci.DisposeAsync();

        Assert.Empty(await SiblingsAsync(resourceNamespace));
    }

    private static async Task<IList<ContainerListResponse>> SiblingsAsync(string resourceNamespace)
    {
        using var client = TestcontainersSettings.OS.DockerEndpointAuthConfig.GetDockerClientBuilder(Guid.NewGuid()).Build();
        return await client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["label"] = new Dictionary<string, bool> { [FlociHost.NamespaceLabel + "=" + resourceNamespace] = true },
            },
        });
    }
}
