using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SesV2ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSimpleEmailServiceV2Client CreateClient()
    {
        return new AmazonSimpleEmailServiceV2Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSimpleEmailServiceV2Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsEmailIdentity()
    {
        using var ses = CreateClient();
        const string email = "test@example.com";

        await ses.CreateEmailIdentityAsync(new CreateEmailIdentityRequest { EmailIdentity = email });

        var identities = await ses.ListEmailIdentitiesAsync(new ListEmailIdentitiesRequest());

        Assert.Contains(identities.EmailIdentities, i => i.IdentityName == email);
    }
}
