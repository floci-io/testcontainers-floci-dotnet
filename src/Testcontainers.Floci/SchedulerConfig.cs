using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EventBridge Scheduler emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithScheduler(SchedulerConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithScheduler(new SchedulerConfig { InvocationEnabled = false })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SchedulerConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether the background dispatcher that fires schedule targets is enabled.
    /// When <see langword="false" />, the scheduler API is CRUD-only. Defaults to <see langword="true" />.
    /// </summary>
    public bool InvocationEnabled { get; init; } = true;

    /// <summary>
    /// Gets how often the dispatcher scans for due schedules, in seconds. Defaults to <c>10</c>.
    /// </summary>
    public long TickIntervalSeconds { get; init; } = 10;

    /// <inheritdoc />
    protected override string ServiceKey => "SCHEDULER";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "INVOCATION_ENABLED"] = InvocationEnabled ? "true" : "false";
        env[prefix + "TICK_INTERVAL_SECONDS"] = TickIntervalSeconds.ToString(CultureInfo.InvariantCulture);
    }
}
