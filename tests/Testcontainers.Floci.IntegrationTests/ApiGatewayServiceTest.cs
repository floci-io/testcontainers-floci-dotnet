using System.Linq;
using System.Threading.Tasks;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ApiGatewayServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithApiGateway(new ApiGatewayConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAPIGatewayClient CreateClient()
    {
        return new AmazonAPIGatewayClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAPIGatewayConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsRestApi()
    {
        using var apigw = CreateClient();
        const string apiName = "test-api";

        await apigw.CreateRestApiAsync(new CreateRestApiRequest { Name = apiName });

        var response = await apigw.GetRestApisAsync(new GetRestApisRequest());

        Assert.Contains(response.Items, api => api.Name == apiName);
    }
}
