using System.Threading.Tasks;
using Amazon.CloudTrail;
using Amazon.CloudTrail.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudTrailServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithCloudTrail(new CloudTrailConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonCloudTrailClient CreateCloudTrailClient()
    {
        return new AmazonCloudTrailClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCloudTrailConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    private AmazonS3Client CreateS3Client()
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
    public async Task CreatesTrailAndTogglesLogging()
    {
        const string bucketName = "cloudtrail-test-bucket";
        const string trailName = "test-trail";

        using var s3 = CreateS3Client();
        using var ct = CreateCloudTrailClient();

        await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });

        await ct.CreateTrailAsync(new CreateTrailRequest
        {
            Name = trailName,
            S3BucketName = bucketName,
        });

        try
        {
            await ct.StartLoggingAsync(new StartLoggingRequest { Name = trailName });

            var statusAfterStart = await ct.GetTrailStatusAsync(new GetTrailStatusRequest { Name = trailName });
            Assert.True(statusAfterStart.IsLogging);

            await ct.StopLoggingAsync(new StopLoggingRequest { Name = trailName });

            var statusAfterStop = await ct.GetTrailStatusAsync(new GetTrailStatusRequest { Name = trailName });
            Assert.False(statusAfterStop.IsLogging);
        }
        finally
        {
            await ct.DeleteTrailAsync(new DeleteTrailRequest { Name = trailName });
        }
    }
}
