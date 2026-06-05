using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's API Gateway V2 emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithApiGatewayV2(ApiGatewayV2Config)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithApiGatewayV2(new ApiGatewayV2Config())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record ApiGatewayV2Config : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APIGATEWAYV2";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // API Gateway V2 has no additional settings beyond the enabled flag.
    }
}
