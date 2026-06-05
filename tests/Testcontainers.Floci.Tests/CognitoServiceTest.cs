using System.Linq;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CognitoServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithCognito(new CognitoConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCognitoIdentityProviderClient CreateClient()
    {
        return new AmazonCognitoIdentityProviderClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCognitoIdentityProviderConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsUserPool()
    {
        using var cognito = CreateClient();
        const string poolName = "test-pool";

        await cognito.CreateUserPoolAsync(new CreateUserPoolRequest { PoolName = poolName });

        var response = await cognito.ListUserPoolsAsync(new ListUserPoolsRequest { MaxResults = 10 });

        Assert.Contains(response.UserPools, p => p.Name == poolName);
    }
}
