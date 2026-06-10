using System.Threading.Tasks;
using Amazon.BCMDataExports;
using Amazon.BCMDataExports.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BcmDataExportsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonBCMDataExportsClient CreateClient()
    {
        return new AmazonBCMDataExportsClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonBCMDataExportsConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task ListsExports()
    {
        using var bcm = CreateClient();

        var response = await bcm.ListExportsAsync(new ListExportsRequest());

        Assert.NotNull(response.Exports);
    }
}
