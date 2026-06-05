using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudFormationConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new CloudFormationConfig();

        Assert.True(config.Enabled);
        Assert.Equal(30L, config.DeletedStackRetentionSeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new CloudFormationConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDFORMATION_ENABLED"]);
        Assert.Equal("30", env["FLOCI_SERVICES_CLOUDFORMATION_DELETED_STACK_RETENTION_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new CloudFormationConfig
        {
            DeletedStackRetentionSeconds = 60,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_CLOUDFORMATION_ENABLED"]);
        Assert.Equal("60", env["FLOCI_SERVICES_CLOUDFORMATION_DELETED_STACK_RETENTION_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new CloudFormationConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_CLOUDFORMATION_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_CLOUDFORMATION_DELETED_STACK_RETENTION_SECONDS", env.Keys);
    }
}
