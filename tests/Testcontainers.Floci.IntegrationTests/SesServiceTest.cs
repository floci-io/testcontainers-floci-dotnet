using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SesServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithSes(new SesConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSimpleEmailServiceClient CreateClient()
    {
        return new AmazonSimpleEmailServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSimpleEmailServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task VerifiesAndListsEmailIdentity()
    {
        using var ses = CreateClient();
        const string email = "test@example.com";

        await ses.VerifyEmailIdentityAsync(new VerifyEmailIdentityRequest { EmailAddress = email });

        var response = await ses.ListIdentitiesAsync(new ListIdentitiesRequest
        {
            IdentityType = IdentityType.EmailAddress,
        });

        Assert.Contains(email, response.Identities);
    }
}
