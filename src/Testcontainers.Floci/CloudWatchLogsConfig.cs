using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CloudWatch Logs emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudWatchLogs(CloudWatchLogsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCloudWatchLogs(new CloudWatchLogsConfig { MaxEventsPerQuery = 5000 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudWatchLogsConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the maximum number of events returned per log query. Defaults to <c>10000</c>.
    /// </summary>
    public int MaxEventsPerQuery { get; init; } = 10000;

    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDWATCHLOGS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MAX_EVENTS_PER_QUERY"] = MaxEventsPerQuery.ToString(CultureInfo.InvariantCulture);
    }
}
