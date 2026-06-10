using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AppConfig Data emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAppConfigData(AppConfigDataConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAppConfigData(new AppConfigDataConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AppConfigDataConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APPCONFIGDATA";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
