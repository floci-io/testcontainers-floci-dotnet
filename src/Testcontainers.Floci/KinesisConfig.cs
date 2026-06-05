using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Kinesis emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithKinesis(KinesisConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithKinesis(new KinesisConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record KinesisConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KINESIS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Kinesis has no additional settings beyond the enabled flag.
    }
}
