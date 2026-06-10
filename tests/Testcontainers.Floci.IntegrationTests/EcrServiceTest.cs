using System.Linq;
using System.Threading.Tasks;
using Amazon.ECR;
using Amazon.ECR.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EcrServiceTest : IAsyncLifetime
{
    private const string RepositoryName = "test-repo";

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEcr(new EcrConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        try
        {
            using var ecr = CreateClient();
            await ecr.DeleteRepositoryAsync(new DeleteRepositoryRequest
            {
                RepositoryName = RepositoryName,
                Force = true,
            });
        }
        catch (AmazonECRException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonECRClient CreateClient()
    {
        return new AmazonECRClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonECRConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndDescribesRepository()
    {
        using var ecr = CreateClient();

        await ecr.CreateRepositoryAsync(new CreateRepositoryRequest { RepositoryName = RepositoryName });

        var response = await ecr.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());

        Assert.Contains(response.Repositories, r => r.RepositoryName == RepositoryName);
    }
}
