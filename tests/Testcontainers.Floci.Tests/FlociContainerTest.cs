using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class FlociContainerTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync()
    {
        return _floci.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _floci.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CreatesAndListsBucketThroughTheAwsSdk()
    {
        // Given
        using var client = new AmazonS3Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = _floci.GetEndpoint(),
                ForcePathStyle = true,
                AuthenticationRegion = _floci.Region,
            });

        const string bucketName = "smoke-test-bucket";

        // When
        await client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
        var buckets = await client.ListBucketsAsync();

        // Then
        Assert.Contains(buckets.Buckets, b => b.BucketName == bucketName);
    }
}
