using System.Linq;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class IamServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonIdentityManagementServiceClient CreateClient()
    {
        return new AmazonIdentityManagementServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonIdentityManagementServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsUser()
    {
        using var iam = CreateClient();
        const string userName = "test-user";

        await iam.CreateUserAsync(new CreateUserRequest { UserName = userName });

        var users = await iam.ListUsersAsync(new ListUsersRequest());

        Assert.Contains(users.Users, u => u.UserName == userName);
    }
}
