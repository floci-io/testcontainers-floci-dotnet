using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Textract emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithTextract(TextractConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithTextract(new TextractConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record TextractConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "TEXTRACT";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
