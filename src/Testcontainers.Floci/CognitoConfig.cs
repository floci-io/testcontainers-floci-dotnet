using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Cognito emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCognito(CognitoConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithCognito(new CognitoConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CognitoConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "COGNITO";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Cognito has no additional settings beyond the enabled flag.
    }
}
