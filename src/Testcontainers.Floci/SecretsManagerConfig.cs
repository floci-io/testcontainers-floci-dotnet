using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Secrets Manager emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSecretsManager(SecretsManagerConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithSecretsManager(new SecretsManagerConfig { RecoveryWindowDays = 7 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SecretsManagerConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the default recovery window in days for deleted secrets. Defaults to <c>30</c>.
    /// </summary>
    public int RecoveryWindowDays { get; init; } = 30;

    /// <inheritdoc />
    protected override string ServiceKey => "SECRETSMANAGER";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DEFAULT_RECOVERY_WINDOW_DAYS"] = RecoveryWindowDays.ToString(CultureInfo.InvariantCulture);
    }
}
