using System.Linq;
using System.Threading.Tasks;
using Amazon.ApiGatewayV2;
using Amazon.ApiGatewayV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class ApiGatewayV2ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithApiGatewayV2(new ApiGatewayV2Config())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonApiGatewayV2Client CreateClient()
    {
        return new AmazonApiGatewayV2Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonApiGatewayV2Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsApi()
    {
        using var apigwv2 = CreateClient();
        const string apiName = "test-api-v2";

        await apigwv2.CreateApiAsync(new CreateApiRequest
        {
            Name = apiName,
            ProtocolType = ProtocolType.HTTP,
        });

        var response = await apigwv2.GetApisAsync(new GetApisRequest());

        Assert.Contains(response.Items, api => api.Name == apiName);
    }
}
