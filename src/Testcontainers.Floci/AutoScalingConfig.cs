using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EC2 Auto Scaling emulation.
/// </summary>
/// <remarks>
/// Floci namespaces Auto Scaling as its own service (<c>FLOCI_SERVICES_AUTOSCALING_*</c>), enabled
/// by default and independent of EC2 — its control plane (launch configurations, Auto Scaling
/// groups) works whether or not EC2 is enabled. It has no settings beyond enablement.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAutoScaling(new AutoScalingConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AutoScalingConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "AUTOSCALING";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
