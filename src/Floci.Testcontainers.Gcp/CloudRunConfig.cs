using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Run emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each service revision and job task as a sibling container, so it needs the Docker socket unless <see cref="GcpMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociGcpBuilder.WithCloudRun(CloudRunConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record CloudRunConfig : GcpMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDRUN";
}
