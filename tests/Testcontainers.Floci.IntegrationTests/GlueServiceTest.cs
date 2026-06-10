using System.Linq;
using System.Threading.Tasks;
using Amazon.Glue;
using Amazon.Glue.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class GlueServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithGlue(new GlueConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonGlueClient CreateClient()
    {
        return new AmazonGlueClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonGlueConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsDatabase()
    {
        using var glue = CreateClient();
        const string dbName = "testdb";

        await glue.CreateDatabaseAsync(new CreateDatabaseRequest
        {
            DatabaseInput = new DatabaseInput { Name = dbName },
        });

        var response = await glue.GetDatabasesAsync(new GetDatabasesRequest());

        Assert.Contains(response.DatabaseList, db => db.Name == dbName);
    }
}
