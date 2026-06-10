namespace Testcontainers.Floci;

public class AwsConfigConfigTest
{
    [Fact]
    public void DefaultsShouldMatchUpstream()
    {
        var config = new AwsConfigConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultEnvOutputShouldMatchUpstream()
    {
        var env = new AwsConfigConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_CONFIGSERVICE_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledOnlyEmitsEnabledFalse()
    {
        var env = new AwsConfigConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_CONFIGSERVICE_ENABLED"]);
        Assert.Single(env);
    }
}
