using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Functions emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithCloudFunctions(CloudFunctionsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record CloudFunctionsConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDFUNCTIONS";
}
