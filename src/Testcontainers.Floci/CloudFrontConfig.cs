using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CloudFront emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudFront(CloudFrontConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCloudFront(new CloudFrontConfig { DomainSuffix = "example.com" })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudFrontConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the domain suffix used for CloudFront distribution domain names.
    /// Defaults to <c>cloudfront.net</c>.
    /// </summary>
    public string DomainSuffix { get; init; } = "cloudfront.net";

    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDFRONT";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DOMAIN_SUFFIX"] = DomainSuffix;
    }
}
