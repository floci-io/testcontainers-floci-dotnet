using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's SSM emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSsm(SsmConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithSsm(new SsmConfig { MaxParameterHistory = 10 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SsmConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the maximum number of parameter history entries to retain. Defaults to <c>5</c>.
    /// </summary>
    public int MaxParameterHistory { get; init; } = 5;

    /// <inheritdoc />
    protected override string ServiceKey => "SSM";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MAX_PARAMETER_HISTORY"] = MaxParameterHistory.ToString(CultureInfo.InvariantCulture);
    }
}
