using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's ACM (Certificate Manager) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAcm(AcmConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAcm(new AcmConfig { ValidationWaitSeconds = 5 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AcmConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the number of seconds to wait before a certificate transitions from
    /// <c>PENDING_VALIDATION</c> to <c>ISSUED</c>. <c>0</c> means immediate. Defaults to <c>0</c>.
    /// </summary>
    public int ValidationWaitSeconds { get; init; } = 0;

    /// <inheritdoc />
    protected override string ServiceKey => "ACM";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "VALIDATION_WAIT_SECONDS"] = ValidationWaitSeconds.ToString(CultureInfo.InvariantCulture);
    }
}
