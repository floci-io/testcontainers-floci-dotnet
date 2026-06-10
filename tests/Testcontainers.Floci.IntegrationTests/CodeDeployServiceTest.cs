using System.Threading.Tasks;
using Amazon.CodeDeploy;
using Amazon.CodeDeploy.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CodeDeployServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCodeDeployClient CreateClient()
    {
        return new AmazonCodeDeployClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCodeDeployConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsApplication()
    {
        using var codeDeploy = CreateClient();
        const string appName = "test-app";

        await codeDeploy.CreateApplicationAsync(new CreateApplicationRequest { ApplicationName = appName });

        var apps = await codeDeploy.ListApplicationsAsync(new ListApplicationsRequest());

        Assert.Contains(apps.Applications, a => a == appName);
    }
}
