using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SqsConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SqsConfig();

        Assert.True(config.Enabled);
        Assert.Equal(30, config.VisibilityTimeout);
        Assert.Equal(262144, config.MaxMessageSize);
        Assert.True(config.ClearFifoDeduplicationCacheOnPurge);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SqsConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SQS_ENABLED"]);
        Assert.Equal("30", env["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
        Assert.Equal("262144", env["FLOCI_SERVICES_SQS_MAX_MESSAGE_SIZE"]);
        Assert.Equal("true", env["FLOCI_SERVICES_SQS_CLEAR_FIFO_DEDUPLICATION_CACHE_ON_PURGE"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new SqsConfig
        {
            VisibilityTimeout = 60,
            MaxMessageSize = 131072,
            ClearFifoDeduplicationCacheOnPurge = false,
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SQS_ENABLED"]);
        Assert.Equal("60", env["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
        Assert.Equal("131072", env["FLOCI_SERVICES_SQS_MAX_MESSAGE_SIZE"]);
        Assert.Equal("false", env["FLOCI_SERVICES_SQS_CLEAR_FIFO_DEDUPLICATION_CACHE_ON_PURGE"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SqsConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_SQS_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SQS_MAX_MESSAGE_SIZE", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SQS_CLEAR_FIFO_DEDUPLICATION_CACHE_ON_PURGE", env.Keys);
    }
}
