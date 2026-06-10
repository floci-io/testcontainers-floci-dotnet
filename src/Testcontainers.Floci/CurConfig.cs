using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Cost and Usage Reports (CUR) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCur(CurConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCur(new CurConfig { EmitMode = "off" })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CurConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the emit mode controlling when CUR Parquet artifacts are produced.
    /// <c>synchronous</c> (default) emits on report definition mutations;
    /// <c>daily</c> emits once per 24h; <c>off</c> disables emission.
    /// </summary>
    public string EmitMode { get; init; } = "synchronous";

    /// <summary>
    /// Gets the S3 bucket used to stage NDJSON row payloads before DuckDB writes the final
    /// Parquet artifact. Defaults to <c>floci-cur-staging</c>.
    /// </summary>
    public string StagingBucket { get; init; } = "floci-cur-staging";

    /// <inheritdoc />
    protected override string ServiceKey => "CUR";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "EMIT_MODE"] = EmitMode;
        env[prefix + "STAGING_BUCKET"] = StagingBucket;
    }
}
