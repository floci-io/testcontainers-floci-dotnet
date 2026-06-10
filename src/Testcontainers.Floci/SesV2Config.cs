using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's SES v2 emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithSesV2(SesV2Config)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithSesV2(new SesV2Config())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SesV2Config : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SES_V2";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
