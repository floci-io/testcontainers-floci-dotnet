using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's S3 emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithS3(S3Config)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithS3(new S3Config { PresignExpirySeconds = 7200 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record S3Config : FlociServiceConfig
{
    /// <summary>
    /// Gets the default presign expiry in seconds. Defaults to <c>3600</c>.
    /// </summary>
    public int PresignExpirySeconds { get; init; } = 3600;

    /// <inheritdoc />
    protected override string ServiceKey => "S3";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DEFAULT_PRESIGN_EXPIRY_SECONDS"] = PresignExpirySeconds.ToString(CultureInfo.InvariantCulture);
    }
}
