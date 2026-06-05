using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's SNS emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSns(SnsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithSns(new SnsConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SnsConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SNS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // SNS has no additional settings beyond the enabled flag.
    }
}
