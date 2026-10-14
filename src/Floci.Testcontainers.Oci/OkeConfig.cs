using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Kubernetes Engine (OKE) emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each cluster as a k3s sibling container, so it needs the Docker socket unless <see cref="OciMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociOciBuilder.WithOke(OkeConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record OkeConfig : OciMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "OKE";
}
