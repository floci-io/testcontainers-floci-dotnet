using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AppConfigDataServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAppConfigClient CreateAppConfigClient()
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

    private AmazonAppConfigDataClient CreateAppConfigDataClient()
    {
        return new AmazonAppConfigDataClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAppConfigDataConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task FetchesConfigurationViaDataPlane()
    {
        using var appConfig = CreateAppConfigClient();
        using var appConfigData = CreateAppConfigDataClient();

        var app = await appConfig.CreateApplicationAsync(new CreateApplicationRequest { Name = "data-test-app" });
        var env = await appConfig.CreateEnvironmentAsync(new CreateEnvironmentRequest
        {
            ApplicationId = app.Id,
            Name = "test-env",
        });
        var profile = await appConfig.CreateConfigurationProfileAsync(new CreateConfigurationProfileRequest
        {
            ApplicationId = app.Id,
            Name = "test-profile",
            LocationUri = "hosted",
        });
        var version = await appConfig.CreateHostedConfigurationVersionAsync(new CreateHostedConfigurationVersionRequest
        {
            ApplicationId = app.Id,
            ConfigurationProfileId = profile.Id,
            Content = new MemoryStream(Encoding.UTF8.GetBytes("{\"key\":\"value\"}")),
            ContentType = "application/json",
        });
        await appConfig.StartDeploymentAsync(new StartDeploymentRequest
        {
            ApplicationId = app.Id,
            EnvironmentId = env.Id,
            ConfigurationProfileId = profile.Id,
            ConfigurationVersion = version.VersionNumber.ToString(),
            DeploymentStrategyId = "AppConfig.AllAtOnce",
        });

        var session = await appConfigData.StartConfigurationSessionAsync(new StartConfigurationSessionRequest
        {
            ApplicationIdentifier = app.Id,
            EnvironmentIdentifier = env.Id,
            ConfigurationProfileIdentifier = profile.Id,
        });

        var config = await appConfigData.GetLatestConfigurationAsync(new GetLatestConfigurationRequest
        {
            ConfigurationToken = session.InitialConfigurationToken,
        });

        Assert.NotNull(config.Configuration);
    }
}
