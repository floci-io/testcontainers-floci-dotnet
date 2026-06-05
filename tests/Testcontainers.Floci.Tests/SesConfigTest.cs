using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SesConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SesConfig();

        Assert.True(config.Enabled);
        Assert.Null(config.SmtpHost);
        Assert.Equal(25, config.SmtpPort);
        Assert.Null(config.SmtpUser);
        Assert.Null(config.SmtpPass);
        Assert.Equal("DISABLED", config.SmtpStarttls);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SesConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SES_ENABLED"]);
        Assert.Equal("25", env["FLOCI_SERVICES_SES_SMTP_PORT"]);
        Assert.Equal("DISABLED", env["FLOCI_SERVICES_SES_SMTP_STARTTLS"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SES_SMTP_HOST", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SES_SMTP_USER", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SES_SMTP_PASS", env.Keys);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new SesConfig
        {
            SmtpHost = "mail.example.com",
            SmtpPort = 587,
            SmtpUser = "user",
            SmtpPass = "pass",
            SmtpStarttls = "REQUIRED",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_SES_ENABLED"]);
        Assert.Equal("mail.example.com", env["FLOCI_SERVICES_SES_SMTP_HOST"]);
        Assert.Equal("587", env["FLOCI_SERVICES_SES_SMTP_PORT"]);
        Assert.Equal("user", env["FLOCI_SERVICES_SES_SMTP_USER"]);
        Assert.Equal("pass", env["FLOCI_SERVICES_SES_SMTP_PASS"]);
        Assert.Equal("REQUIRED", env["FLOCI_SERVICES_SES_SMTP_STARTTLS"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new SesConfig { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_SES_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_SES_SMTP_PORT", env.Keys);
        Assert.DoesNotContain("FLOCI_SERVICES_SES_SMTP_STARTTLS", env.Keys);
    }
}
