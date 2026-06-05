using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class Ec2ServiceTest : IAsyncLifetime
{
    // Mock mode: instances go straight to RUNNING without backing containers — deterministic,
    // no socket needed, no leaks.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEc2(new Ec2Config { Mock = true })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonEC2Client CreateClient()
    {
        return new AmazonEC2Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonEC2Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task RunsAndDescribesInstance()
    {
        using var ec2 = CreateClient();

        var runResponse = await ec2.RunInstancesAsync(new RunInstancesRequest
        {
            ImageId = "ami-12345678",
            MinCount = 1,
            MaxCount = 1,
        });

        var instanceId = runResponse.Reservation.Instances.Single().InstanceId;

        var describeResponse = await ec2.DescribeInstancesAsync(new DescribeInstancesRequest
        {
            InstanceIds = new List<string> { instanceId },
        });

        var found = describeResponse.Reservations
            .SelectMany(r => r.Instances)
            .Any(i => i.InstanceId == instanceId);

        Assert.True(found, $"Instance {instanceId} not found in DescribeInstances.");
    }
}
