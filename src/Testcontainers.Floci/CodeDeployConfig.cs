using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CodeDeploy emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCodeDeploy(CodeDeployConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCodeDeploy(new CodeDeployConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CodeDeployConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CODEDEPLOY";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
    }
}
