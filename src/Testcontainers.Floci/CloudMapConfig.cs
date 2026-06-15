using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Cloud Map (service discovery) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudMap(CloudMapConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCloudMap(new CloudMapConfig { OperationCompletionDelaySeconds = 0 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudMapConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the delay in seconds before asynchronous operations transition from
    /// <c>PENDING</c> to <c>SUCCESS</c>. Defaults to <c>0</c> (immediate).
    /// </summary>
    public int OperationCompletionDelaySeconds { get; init; } = 0;

    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDMAP";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "OPERATION_COMPLETION_DELAY_SECONDS"] = OperationCompletionDelaySeconds.ToString(CultureInfo.InvariantCulture);
    }
}
