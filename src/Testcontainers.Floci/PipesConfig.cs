using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EventBridge Pipes emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithPipes(PipesConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithPipes(new PipesConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record PipesConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "PIPES";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
