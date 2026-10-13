using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Scheduler emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithScheduler(GcpSchedulerConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GcpSchedulerConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SCHEDULER";
}
