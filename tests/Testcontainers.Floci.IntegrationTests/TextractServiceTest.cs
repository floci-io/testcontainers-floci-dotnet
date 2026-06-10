using System.Threading.Tasks;
using Amazon.Textract;
using Amazon.Textract.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TextractServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonTextractClient CreateClient()
    {
        return new AmazonTextractClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonTextractConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task ListsAdapters()
    {
        using var textract = CreateClient();

        var response = await textract.ListAdaptersAsync(new ListAdaptersRequest());

        Assert.NotNull(response.Adapters);
    }
}
