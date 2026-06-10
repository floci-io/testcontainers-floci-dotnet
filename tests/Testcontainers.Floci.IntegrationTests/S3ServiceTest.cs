using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class S3ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonS3Client CreateClient()
    {
        return new AmazonS3Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = _floci.GetEndpoint(),
                ForcePathStyle = true,
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsBucket()
    {
        using var s3 = CreateClient();
        const string bucketName = "test-bucket";

        await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });

        var buckets = await s3.ListBucketsAsync();

        Assert.Contains(buckets.Buckets, b => b.BucketName == bucketName);
    }

    [Fact]
    public async Task PutsAndGetsObject()
    {
        using var s3 = CreateClient();
        const string bucketName = "test-roundtrip-bucket";
        const string key = "hello.txt";
        const string content = "Hello from Floci S3!";

        await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentBody = content,
        });

        var response = await s3.GetObjectAsync(new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key,
        });

        using var reader = new StreamReader(response.ResponseStream, Encoding.UTF8);
        var retrieved = await reader.ReadToEndAsync();

        Assert.Equal(content, retrieved);
    }
}
