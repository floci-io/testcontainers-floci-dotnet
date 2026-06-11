using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class Ec2ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new Ec2Config();

        Assert.True(config.Enabled);
        Assert.False(config.Mock);
        Assert.Equal(9169, config.ImdsPort);
        Assert.Equal(2200, config.SshPortRangeStart);
        Assert.Equal(2299, config.SshPortRangeEnd);
    }

    [Fact]
    public void RequiresDockerAccessOnlyInRealMode()
    {
        Assert.True(new Ec2Config().RequiresDockerAccess);
        Assert.False(new Ec2Config { Mock = true }.RequiresDockerAccess);
    }

    [Fact]
    public void PublishesTheImdsPort()
    {
        Assert.Equal(new[] { 9169 }, new Ec2Config().FixedHostPorts);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new Ec2Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EC2_ENABLED"]);
        Assert.Equal("false", env["FLOCI_SERVICES_EC2_MOCK"]);
        Assert.Equal("9169", env["FLOCI_SERVICES_EC2_IMDS_PORT"]);
        Assert.Equal("2200", env["FLOCI_SERVICES_EC2_SSH_PORT_RANGE_START"]);
        Assert.Equal("2299", env["FLOCI_SERVICES_EC2_SSH_PORT_RANGE_END"]);
        // Auto Scaling is no longer coupled to EC2 — see AutoScalingConfig.
        Assert.DoesNotContain("FLOCI_SERVICES_AUTOSCALING_ENABLED", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new Ec2Config
        {
            Mock = true,
            ImdsPort = 9200,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_EC2_MOCK"]);
        Assert.Equal("9200", env["FLOCI_SERVICES_EC2_IMDS_PORT"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new Ec2Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_EC2_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_EC2_MOCK", env.Keys);
    }
}
