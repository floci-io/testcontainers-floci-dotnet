using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SesV2ServiceTest : IAsyncLifetime
{
    // Enable SES v2 explicitly through the builder so the FLOCI_SERVICES_SES_V2_* env keys are
    // actually emitted and exercised, rather than relying on Floci's default-on behaviour.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithSesV2(new SesV2Config())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private static AmazonSimpleEmailServiceV2Client CreateClient(FlociContainer floci)
    {
        return new AmazonSimpleEmailServiceV2Client(
            floci.AccessKey,
            floci.SecretKey,
            new AmazonSimpleEmailServiceV2Config
            {
                ServiceURL = floci.GetEndpoint(),
                AuthenticationRegion = floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsEmailIdentity()
    {
        using var ses = CreateClient(_floci);
        const string email = "test@example.com";

        await ses.CreateEmailIdentityAsync(new CreateEmailIdentityRequest { EmailIdentity = email });

        var identities = await ses.ListEmailIdentitiesAsync(new ListEmailIdentitiesRequest());

        Assert.Contains(identities.EmailIdentities, i => i.IdentityName == email);
    }

    [Fact]
    public async Task DisablingSesV2RejectsRequests()
    {
        // Conclusive check on the ServiceKey token: if "SES_V2" is the correct env-var segment,
        // emitting FLOCI_SERVICES_SES_V2_ENABLED=false turns the service off and the call is
        // rejected. If the token were wrong, the disable would be silently ignored, the service
        // would stay on (Floci defaults it on), and the call would succeed — failing this test.
        await using var floci = new FlociBuilder(TestImages.Floci)
            .WithSesV2(new SesV2Config { Enabled = false })
            .Build();
        await floci.StartAsync();

        using var ses = CreateClient(floci);

        await Assert.ThrowsAnyAsync<AmazonSimpleEmailServiceV2Exception>(
            () => ses.ListEmailIdentitiesAsync(new ListEmailIdentitiesRequest()));
    }
}
