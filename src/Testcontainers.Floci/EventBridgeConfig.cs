using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EventBridge emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithEventBridge(EventBridgeConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithEventBridge(new EventBridgeConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record EventBridgeConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "EVENTBRIDGE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // EventBridge has no additional settings beyond the enabled flag.
    }
}
