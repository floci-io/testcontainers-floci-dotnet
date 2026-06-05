using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's KMS emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithKms(KmsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithKms(new KmsConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record KmsConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KMS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // KMS has no additional settings beyond the enabled flag.
    }
}
