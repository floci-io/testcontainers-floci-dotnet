using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CloudTrail emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudTrail(CloudTrailConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithCloudTrail(new CloudTrailConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudTrailConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDTRAIL";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // CloudTrail has no additional settings beyond the enabled flag.
    }
}
