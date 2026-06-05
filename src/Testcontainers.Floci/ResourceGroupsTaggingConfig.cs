using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Resource Groups Tagging emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithResourceGroupsTagging(ResourceGroupsTaggingConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithResourceGroupsTagging(new ResourceGroupsTaggingConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record ResourceGroupsTaggingConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "TAGGING";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Resource Groups Tagging has no additional settings beyond the enabled flag.
    }
}
