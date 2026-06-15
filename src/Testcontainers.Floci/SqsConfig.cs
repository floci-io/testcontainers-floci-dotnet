using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's SQS emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSqs(SqsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithSqs(new SqsConfig { VisibilityTimeout = 60, MaxMessageSize = 131072 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SqsConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the default visibility timeout in seconds. Defaults to <c>30</c>.
    /// </summary>
    public int VisibilityTimeout { get; init; } = 30;

    /// <summary>
    /// Gets the maximum message size in bytes. Defaults to <c>1048576</c> (1 MiB).
    /// </summary>
    public int MaxMessageSize { get; init; } = 1048576;

    /// <summary>
    /// Gets a value indicating whether the FIFO deduplication cache is cleared on queue purge.
    /// Defaults to <see langword="false" />.
    /// </summary>
    public bool ClearFifoDeduplicationCacheOnPurge { get; init; } = false;

    /// <inheritdoc />
    protected override string ServiceKey => "SQS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DEFAULT_VISIBILITY_TIMEOUT"] = VisibilityTimeout.ToString(CultureInfo.InvariantCulture);
        env[prefix + "MAX_MESSAGE_SIZE"] = MaxMessageSize.ToString(CultureInfo.InvariantCulture);
        env[prefix + "CLEAR_FIFO_DEDUPLICATION_CACHE_ON_PURGE"] = ClearFifoDeduplicationCacheOnPurge ? "true" : "false";
    }
}
