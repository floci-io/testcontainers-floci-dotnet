using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Container Registry emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs registries in a sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithAcr(AcrConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record AcrConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcrConfig" /> class.
    /// </summary>
    public AcrConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "ACR";
}
