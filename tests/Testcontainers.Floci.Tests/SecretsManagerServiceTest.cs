using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SecretsManagerServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSecretsManagerClient CreateClient()
    {
        return new AmazonSecretsManagerClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSecretsManagerConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndGetsSecret()
    {
        using var secretsManager = CreateClient();
        const string name = "test-secret";
        const string secretString = "s3cr3t";

        await secretsManager.CreateSecretAsync(new CreateSecretRequest
        {
            Name = name,
            SecretString = secretString,
        });

        var response = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = name });

        Assert.Equal(secretString, response.SecretString);
    }
}
