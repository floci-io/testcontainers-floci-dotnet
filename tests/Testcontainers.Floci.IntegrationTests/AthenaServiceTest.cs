using System.Threading.Tasks;
using Amazon.Athena;
using Amazon.Athena.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AthenaServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAthenaClient CreateClient()
    {
        return new AmazonAthenaClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAthenaConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndGetsWorkGroup()
    {
        using var athena = CreateClient();
        const string workGroupName = "test-workgroup";

        await athena.CreateWorkGroupAsync(new CreateWorkGroupRequest { Name = workGroupName });

        var response = await athena.GetWorkGroupAsync(new GetWorkGroupRequest { WorkGroup = workGroupName });

        Assert.Equal(workGroupName, response.WorkGroup.Name);
    }
}
