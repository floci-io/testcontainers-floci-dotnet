using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Transcribe emulation.
/// </summary>
/// <remarks>
/// Transcribe has no settings beyond enablement.
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithTranscribe(new TranscribeConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record TranscribeConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "TRANSCRIBE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
