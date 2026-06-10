using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AppSync emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAppSync(AppSyncConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAppSync(new AppSyncConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AppSyncConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APPSYNC";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
