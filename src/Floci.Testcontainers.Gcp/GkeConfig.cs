using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Google Kubernetes Engine (GKE) emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each cluster as a k3s sibling container, so it needs the Docker socket unless <see cref="GcpMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociGcpBuilder.WithGke(GkeConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GkeConfig : GcpMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "GKE";
}
