using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CloudWatch Metrics emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudWatchMetrics(CloudWatchMetricsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCloudWatchMetrics(new CloudWatchMetricsConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudWatchMetricsConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDWATCHMETRICS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // CloudWatch Metrics has no additional settings beyond the enabled flag.
    }
}
