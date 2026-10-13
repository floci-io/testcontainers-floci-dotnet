using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Container Instances emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service starts each container group as real containers. It is mocked by default, so it needs the Docker socket only when the mock is switched off.
/// Apply via <see cref="FlociAzBuilder.WithAci(AciConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record AciConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AciConfig" /> class.
    /// </summary>
    public AciConfig()
        : base(mockedByDefault: true)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "ACI";
}
