using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's WAF v2 emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithWafV2(WafV2Config)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithWafV2(new WafV2Config())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record WafV2Config : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "WAFV2";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // WAF v2 is an enabled-only service; there are no additional settings beyond the enabled flag.
    }
}
