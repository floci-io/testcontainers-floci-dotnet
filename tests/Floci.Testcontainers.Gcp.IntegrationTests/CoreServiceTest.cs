using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Floci.Testcontainers.Gcp;
using Google.Api.Gax;
using Google.Cloud.Storage.V1;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

public sealed class CoreServiceTest : IClassFixture<FlociGcpFixture>
{
    private readonly FlociGcpContainer _floci;

    public CoreServiceTest(FlociGcpFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task StartsHealthy()
    {
        using var http = new HttpClient();
        using var response = await http.GetAsync(_floci.GetEndpoint() + GcpCloud.Descriptor.HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(GcpCloud.DefaultProjectId, _floci.ProjectId);
    }

    [Fact]
    public void EmulatorEnvPointsEveryLibraryAtTheContainer()
    {
        var hostAndPort = _floci.GetHostAndPort();
        Assert.Equal("http://" + hostAndPort, _floci.GetEndpoint());

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["PUBSUB_EMULATOR_HOST"] = hostAndPort,
                ["FIRESTORE_EMULATOR_HOST"] = hostAndPort,
                ["DATASTORE_EMULATOR_HOST"] = hostAndPort,
                ["SECRET_MANAGER_EMULATOR_HOST"] = hostAndPort,
                ["FIREBASE_AUTH_EMULATOR_HOST"] = hostAndPort,
                ["LOGGING_EMULATOR_HOST"] = hostAndPort,
                ["KMS_EMULATOR_HOST"] = hostAndPort,
                ["STORAGE_EMULATOR_HOST"] = "http://" + hostAndPort,
                ["STORAGE_EMULATOR_HOST_GRPC"] = hostAndPort,
                ["GOOGLE_CLOUD_PROJECT"] = "floci-local",
            },
            _floci.GetEmulatorEnv());
    }

    [Fact]
    public async Task ResetWipesStateWithoutRestarting()
    {
        var storage = await new StorageClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOnly }.BuildAsync();
        await storage.CreateBucketAsync(_floci.ProjectId, "reset-me");
        Assert.Contains(await BucketNamesAsync(storage), name => name == "reset-me");

        await _floci.ResetAsync();

        Assert.DoesNotContain(await BucketNamesAsync(storage), name => name == "reset-me");
    }

    [Fact]
    public async Task HasAUniqueResourceNamespacePerStartAndReportsTheConfiguredProject()
    {
        await using var other = new FlociGcpBuilder(TestImages.FlociGcp).WithProjectId("other-project").Build();
        await other.StartAsync();

        Assert.Matches("^tc-[0-9a-f]{8}$", _floci.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", other.ResourceNamespace);
        Assert.NotEqual(_floci.ResourceNamespace, other.ResourceNamespace);

        Assert.Equal("other-project", other.ProjectId);
        Assert.Equal("other-project", other.GetEmulatorEnv()["GOOGLE_CLOUD_PROJECT"]);

        using var http = new HttpClient();
        var info = await http.GetStringAsync(other.GetEndpoint() + "/_floci-gcp/info");
        Assert.Contains("\"defaultProject\":\"other-project\"", info);
    }

    private async Task<List<string>> BucketNamesAsync(StorageClient storage)
    {
        var names = new List<string>();
        await foreach (var bucket in storage.ListBucketsAsync(_floci.ProjectId))
        {
            names.Add(bucket.Name);
        }

        return names;
    }
}
