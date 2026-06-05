using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Firehose emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithFirehose(FirehoseConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithFirehose(new FirehoseConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record FirehoseConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "FIREHOSE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Firehose has no additional settings beyond the enabled flag.
    }
}
