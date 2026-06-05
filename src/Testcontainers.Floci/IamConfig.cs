using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's IAM emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithIam(IamConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithIam(new IamConfig { EnforcementEnabled = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record IamConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether IAM enforcement is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool EnforcementEnabled { get; init; } = false;

    /// <inheritdoc />
    protected override string ServiceKey => "IAM";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "ENFORCEMENT_ENABLED"] = EnforcementEnabled ? "true" : "false";
    }
}
