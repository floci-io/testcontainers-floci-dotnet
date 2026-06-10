namespace Testcontainers.Floci;

public class TransferFamilyConfigTest
{
    [Fact]
    public void DefaultsShouldMatchUpstream()
    {
        var config = new TransferFamilyConfig();
        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultEnvOutputShouldMatchUpstream()
    {
        var env = new TransferFamilyConfig().BuildEnvironment();
        Assert.Equal("true", env["FLOCI_SERVICES_TRANSFER_ENABLED"]);
        Assert.Single(env);
    }

    [Fact]
    public void DisabledOnlyEmitsEnabledFalse()
    {
        var env = new TransferFamilyConfig { Enabled = false }.BuildEnvironment();
        Assert.Equal("false", env["FLOCI_SERVICES_TRANSFER_ENABLED"]);
        Assert.Single(env);
    }
}
