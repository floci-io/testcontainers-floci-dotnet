using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Pricing emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithPricing(PricingConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithPricing(new PricingConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record PricingConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the filesystem directory that overrides the bundled pricing snapshot, or
    /// <see langword="null" /> to use the built-in snapshot.
    /// </summary>
    public string? SnapshotPath { get; init; } = null;

    /// <inheritdoc />
    protected override string ServiceKey => "PRICING";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        if (SnapshotPath is not null)
        {
            env[prefix + "SNAPSHOT_PATH"] = SnapshotPath;
        }
    }
}
