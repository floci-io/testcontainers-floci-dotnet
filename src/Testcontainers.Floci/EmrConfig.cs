using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EMR (Elastic MapReduce) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithEmr(EmrConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithEmr(new EmrConfig { DefaultReleaseLabel = "emr-7.5.0" })
///     .Build();
/// </code>
/// EMR is a pure management-API service in Floci (state-machine based, no sibling containers).
/// </remarks>
[PublicAPI]
public sealed record EmrConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the default EMR release label applied to new clusters. Defaults to <c>"emr-7.5.0"</c>.
    /// </summary>
    public string DefaultReleaseLabel { get; init; } = "emr-7.5.0";

    /// <summary>
    /// Gets the simulated delay in seconds before a cluster transitions to the WAITING state.
    /// Defaults to <c>0</c> (instant).
    /// </summary>
    public int ClusterStartupDelaySeconds { get; init; } = 0;

    /// <inheritdoc />
    protected override string ServiceKey => "EMR";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DEFAULT_RELEASE_LABEL"] = DefaultReleaseLabel;
        env[prefix + "CLUSTER_STARTUP_DELAY_SECONDS"] = ClusterStartupDelaySeconds.ToString(CultureInfo.InvariantCulture);
    }
}
