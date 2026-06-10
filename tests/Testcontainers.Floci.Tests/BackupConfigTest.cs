using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BackupConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new BackupConfig();

        Assert.True(config.Enabled);
        Assert.Equal(3, config.JobCompletionDelaySeconds);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new BackupConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BACKUP_ENABLED"]);
        Assert.Equal("3", env["FLOCI_SERVICES_BACKUP_JOB_COMPLETION_DELAY_SECONDS"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new BackupConfig { JobCompletionDelaySeconds = 0 }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_BACKUP_ENABLED"]);
        Assert.Equal("0", env["FLOCI_SERVICES_BACKUP_JOB_COMPLETION_DELAY_SECONDS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new BackupConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_BACKUP_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_BACKUP_JOB_COMPLETION_DELAY_SECONDS", env.Keys);
    }
}
