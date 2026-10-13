using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Kubernetes Service (AKS) emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each cluster as a k3s sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithAks(AksConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record AksConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AksConfig" /> class.
    /// </summary>
    public AksConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "AKS";
}
