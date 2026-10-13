using System.IO;
using System.Text;
using System.Threading.Tasks;
using Floci.Testcontainers.Gcp;
using Google.Api.Gax;
using Google.Cloud.Storage.V1;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

public sealed class GcsServiceTest : IClassFixture<FlociGcpFixture>
{
    private readonly FlociGcpContainer _floci;

    public GcsServiceTest(FlociGcpFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task UploadsAndDownloadsAnObjectThroughTheEmulatorEnv()
    {
        // STORAGE_EMULATOR_HOST, set by the fixture from GetEmulatorEnv().
        var storage = await new StorageClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOnly }.BuildAsync();

        Assert.Equal("hello from floci", await RoundTripAsync(storage, "orders-env"));
    }

    [Fact]
    public async Task UploadsAndDownloadsAnObjectThroughTheEndpoint()
    {
        var storage = await new StorageClientBuilder
        {
            BaseUri = _floci.GetEndpoint() + "/storage/v1/",
            UnauthenticatedAccess = true,
        }.BuildAsync();

        Assert.Equal("hello from floci", await RoundTripAsync(storage, "orders-endpoint"));
    }

    private async Task<string> RoundTripAsync(StorageClient storage, string bucket)
    {
        await storage.CreateBucketAsync(_floci.ProjectId, bucket);
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello from floci"));
        await storage.UploadObjectAsync(bucket, "order-1.txt", "text/plain", content);

        using var downloaded = new MemoryStream();
        await storage.DownloadObjectAsync(bucket, "order-1.txt", downloaded);
        return Encoding.UTF8.GetString(downloaded.ToArray());
    }
}
