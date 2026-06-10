using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Bedrock Runtime emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithBedrockRuntime(BedrockRuntimeConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithBedrockRuntime(new BedrockRuntimeConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record BedrockRuntimeConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "BEDROCK_RUNTIME";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
