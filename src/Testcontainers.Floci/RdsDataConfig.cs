using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's RDS Data API emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithRdsData(RdsDataConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithRdsData(new RdsDataConfig())
///     .Build();
/// </code>
/// RDS Data API is enabled by default (alongside RDS). It exposes <c>ExecuteStatement</c>,
/// <c>BeginTransaction</c>, <c>CommitTransaction</c>, and <c>RollbackTransaction</c> against
/// Floci-managed local MySQL or MariaDB clusters. Use <see cref="TransactionTtlSeconds" /> to
/// control how long open transactions are held before they are automatically rolled back and
/// their connections closed.
/// </remarks>
[PublicAPI]
public sealed record RdsDataConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the number of seconds an open transaction is kept alive without activity before it
    /// is automatically rolled back. Defaults to <c>180</c>.
    /// </summary>
    public long TransactionTtlSeconds { get; init; } = 180;

    /// <inheritdoc />
    protected override string ServiceKey => "RDS_DATA";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "TRANSACTION_TTL_SECONDS"] = TransactionTtlSeconds.ToString(CultureInfo.InvariantCulture);
    }
}
