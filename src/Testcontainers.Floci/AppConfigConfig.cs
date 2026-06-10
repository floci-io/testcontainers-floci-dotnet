using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AppConfig emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAppConfig(AppConfigConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAppConfig(new AppConfigConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AppConfigConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APPCONFIG";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
