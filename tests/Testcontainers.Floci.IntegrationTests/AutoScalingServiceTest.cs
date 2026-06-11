using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AutoScalingServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithAutoScaling(new AutoScalingConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonAutoScalingClient CreateClient()
    {
        return new AmazonAutoScalingClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonAutoScalingConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndDescribesAutoScalingGroup()
    {
        using var autoScaling = CreateClient();
        const string launchConfig = "lc-test";
        const string groupName = "asg-test";

        // A launch configuration is an Auto Scaling-native resource (no EC2 RunInstances needed),
        // so this exercises the service standalone — Auto Scaling is enabled independently of EC2.
        await autoScaling.CreateLaunchConfigurationAsync(new CreateLaunchConfigurationRequest
        {
            LaunchConfigurationName = launchConfig,
            ImageId = "ami-12345678",
            InstanceType = "t2.micro",
        });

        await autoScaling.CreateAutoScalingGroupAsync(new CreateAutoScalingGroupRequest
        {
            AutoScalingGroupName = groupName,
            LaunchConfigurationName = launchConfig,
            MinSize = 0,
            MaxSize = 1,
            AvailabilityZones = new List<string> { "us-east-1a" },
        });

        var response = await autoScaling.DescribeAutoScalingGroupsAsync(new DescribeAutoScalingGroupsRequest());

        Assert.Contains(response.AutoScalingGroups, g => g.AutoScalingGroupName == groupName);
    }
}
