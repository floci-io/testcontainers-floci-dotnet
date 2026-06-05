using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's SES emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSes(SesConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithSes(new SesConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SesConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the SMTP server host for email relay. <see langword="null" /> disables relay (emails stored only).
    /// Defaults to <see langword="null" />.
    /// </summary>
    public string? SmtpHost { get; init; } = null;

    /// <summary>
    /// Gets the SMTP server port. Defaults to <c>25</c>.
    /// </summary>
    public int SmtpPort { get; init; } = 25;

    /// <summary>
    /// Gets the SMTP authentication username. <see langword="null" /> disables authentication.
    /// Defaults to <see langword="null" />.
    /// </summary>
    public string? SmtpUser { get; init; } = null;

    /// <summary>
    /// Gets the SMTP authentication password. Defaults to <see langword="null" />.
    /// </summary>
    public string? SmtpPass { get; init; } = null;

    /// <summary>
    /// Gets the STARTTLS mode: <c>DISABLED</c>, <c>OPTIONAL</c>, or <c>REQUIRED</c>. Defaults to <c>DISABLED</c>.
    /// </summary>
    public string SmtpStarttls { get; init; } = "DISABLED";

    /// <inheritdoc />
    protected override string ServiceKey => "SES";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        if (SmtpHost != null)
        {
            env[prefix + "SMTP_HOST"] = SmtpHost;
        }

        env[prefix + "SMTP_PORT"] = SmtpPort.ToString(CultureInfo.InvariantCulture);

        if (SmtpUser != null)
        {
            env[prefix + "SMTP_USER"] = SmtpUser;
        }

        if (SmtpPass != null)
        {
            env[prefix + "SMTP_PASS"] = SmtpPass;
        }

        env[prefix + "SMTP_STARTTLS"] = SmtpStarttls;
    }
}
