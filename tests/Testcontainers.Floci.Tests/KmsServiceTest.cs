using System.Linq;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class KmsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonKeyManagementServiceClient CreateClient()
    {
        return new AmazonKeyManagementServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonKeyManagementServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsKey()
    {
        using var kms = CreateClient();

        var created = await kms.CreateKeyAsync(new CreateKeyRequest());
        var keyId = created.KeyMetadata.KeyId;

        var keys = await kms.ListKeysAsync(new ListKeysRequest());

        Assert.Contains(keys.Keys, k => k.KeyId == keyId);
    }
}
