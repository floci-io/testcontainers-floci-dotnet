using System.Threading.Tasks;
using Amazon.AppSync;
using Amazon.AppSync.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AppSyncServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAppSyncClient CreateClient()
    {
        return new AmazonAppSyncClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAppSyncConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsGraphqlApi()
    {
        using var appSync = CreateClient();
        const string apiName = "test-api";

        await appSync.CreateGraphqlApiAsync(new CreateGraphqlApiRequest
        {
            Name = apiName,
            AuthenticationType = AuthenticationType.API_KEY,
        });

        var apis = await appSync.ListGraphqlApisAsync(new ListGraphqlApisRequest());

        Assert.Contains(apis.GraphqlApis, a => a.Name == apiName);
    }
}
