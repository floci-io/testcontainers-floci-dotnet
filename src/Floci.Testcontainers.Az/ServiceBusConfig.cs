using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Service Bus emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs its broker in a sibling container. It is mocked by default, so it needs the Docker socket only when the mock is switched off.
/// Apply via <see cref="FlociAzBuilder.WithServiceBus(ServiceBusConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record ServiceBusConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusConfig" /> class.
    /// </summary>
    public ServiceBusConfig()
        : base(mockedByDefault: true)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "SERVICE_BUS";
}
