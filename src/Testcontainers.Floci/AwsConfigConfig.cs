using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's AWS Config emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithAwsConfig(AwsConfigConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithAwsConfig(new AwsConfigConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record AwsConfigConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CONFIGSERVICE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix) { }
}
