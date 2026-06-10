using System.Threading.Tasks;
using Amazon.Pipes;
using Amazon.Pipes.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class PipesServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonPipesClient CreateClient()
    {
        return new AmazonPipesClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonPipesConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task ListsPipes()
    {
        using var pipes = CreateClient();

        var response = await pipes.ListPipesAsync(new ListPipesRequest());

        Assert.NotNull(response.Pipes);
    }
}
