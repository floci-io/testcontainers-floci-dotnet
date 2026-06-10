using System.Threading.Tasks;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AppConfigServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAppConfigClient CreateClient()
    {
        return new AmazonAppConfigClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAppConfigConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsApplication()
    {
        using var appConfig = CreateClient();
        const string appName = "test-app";

        await appConfig.CreateApplicationAsync(new CreateApplicationRequest { Name = appName });

        var apps = await appConfig.ListApplicationsAsync(new ListApplicationsRequest());

        Assert.Contains(apps.Items, a => a.Name == appName);
    }
}
