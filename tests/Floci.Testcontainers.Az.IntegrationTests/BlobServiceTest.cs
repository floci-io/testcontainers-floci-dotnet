using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class BlobServiceTest : IClassFixture<FlociAzFixture>
{
    private readonly FlociAzContainer _floci;

    public BlobServiceTest(FlociAzFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task UploadsAndDownloadsABlob()
    {
        var blobs = new BlobServiceClient(_floci.GetStorageConnectionString());
        var container = (await blobs.CreateBlobContainerAsync("orders")).Value;
        Assert.StartsWith(_floci.GetBlobEndpoint(), container.Uri.ToString());

        var blob = container.GetBlobClient("order-1.txt");
        await blob.UploadAsync(BinaryData.FromString("hello from floci"));

        var downloaded = await blob.DownloadContentAsync();
        Assert.Equal("hello from floci", downloaded.Value.Content.ToString());
    }
}
