using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's BCM Data Exports emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithBcmDataExports(BcmDataExportsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithBcmDataExports(new BcmDataExportsConfig { EmitMode = "off" })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record BcmDataExportsConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the emit mode controlling when BCM Data Exports Parquet artifacts are produced.
    /// <c>synchronous</c> (default) emits on export mutations; <c>daily</c> emits once per 24h;
    /// <c>off</c> disables emission.
    /// </summary>
    public string EmitMode { get; init; } = "synchronous";

    /// <inheritdoc />
    protected override string ServiceKey => "BCM_DATA_EXPORTS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "EMIT_MODE"] = EmitMode;
    }
}
