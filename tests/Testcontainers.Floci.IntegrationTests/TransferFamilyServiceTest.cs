using System.Threading.Tasks;
using Amazon.Transfer;
using Amazon.Transfer.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TransferFamilyServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithTransferFamily(new TransferFamilyConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    [Fact]
    public async Task CreateServer_ThenListServers_ReturnsIt()
    {
        var client = new AmazonTransferClient(
            new Amazon.Runtime.BasicAWSCredentials(_floci.AccessKey, _floci.SecretKey),
            new AmazonTransferConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });

        await client.CreateServerAsync(new CreateServerRequest());
        var list = await client.ListServersAsync(new ListServersRequest());
        Assert.NotEmpty(list.Servers);
    }
}
