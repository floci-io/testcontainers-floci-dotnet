using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's API Gateway (v1) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithApiGateway(ApiGatewayConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithApiGateway(new ApiGatewayConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record ApiGatewayConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "APIGATEWAY";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // API Gateway has no additional settings beyond the enabled flag.
    }
}
