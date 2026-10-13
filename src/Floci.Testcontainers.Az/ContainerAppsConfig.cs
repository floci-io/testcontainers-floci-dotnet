using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Container Apps emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service starts the application containers. It is mocked by default, so it needs the Docker socket only when the mock is switched off.
/// Apply via <see cref="FlociAzBuilder.WithContainerApps(ContainerAppsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ContainerAppsConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerAppsConfig" /> class.
    /// </summary>
    public ContainerAppsConfig()
        : base(mockedByDefault: true)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "CONTAINER_APPS";
}
