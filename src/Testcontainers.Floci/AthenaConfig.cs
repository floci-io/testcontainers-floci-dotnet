using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Athena emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAthena(AthenaConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithAthena(new AthenaConfig { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AthenaConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether Athena operates in mock mode (no real DuckDB backend).
    /// Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; } = false;

    /// <inheritdoc />
    protected override string ServiceKey => "ATHENA";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
    }
}
