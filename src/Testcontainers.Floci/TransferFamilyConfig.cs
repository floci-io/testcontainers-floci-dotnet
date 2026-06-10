using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Transfer Family emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithTransferFamily(TransferFamilyConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithTransferFamily(new TransferFamilyConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record TransferFamilyConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "TRANSFER";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Transfer Family has no additional settings beyond the enabled flag.
    }
}
