using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Event Hubs emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs its brokers in sibling containers. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithEventHub(EventHubConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record EventHubConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventHubConfig" /> class.
    /// </summary>
    public EventHubConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "EVENT_HUB";
}
