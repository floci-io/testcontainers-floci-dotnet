using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Glue emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithGlue(GlueConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithGlue(new GlueConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record GlueConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "GLUE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Glue has no additional settings beyond the enabled flag.
    }
}
