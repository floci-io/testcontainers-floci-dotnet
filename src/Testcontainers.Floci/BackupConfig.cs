using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AWS Backup emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithBackup(BackupConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithBackup(new BackupConfig { JobCompletionDelaySeconds = 0 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record BackupConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the number of seconds to wait before marking a backup job as completed.
    /// Defaults to <c>3</c>.
    /// </summary>
    public int JobCompletionDelaySeconds { get; init; } = 3;

    /// <inheritdoc />
    protected override string ServiceKey => "BACKUP";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "JOB_COMPLETION_DELAY_SECONDS"] = JobCompletionDelaySeconds.ToString(CultureInfo.InvariantCulture);
    }
}
