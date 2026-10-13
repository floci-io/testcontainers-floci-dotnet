using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Base type of the Azure services that have a mock switch (<c>…_MOCKED</c>): mocked, the service
/// answers its management calls without spawning containers, so it does not need the Docker socket.
/// </summary>
[PublicAPI]
public abstract record AzMockableServiceConfig : AzServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzMockableServiceConfig" /> class.
    /// </summary>
    /// <param name="mockedByDefault">The emulator's default for the service's mock switch.</param>
    protected AzMockableServiceConfig(bool mockedByDefault)
    {
        Mocked = mockedByDefault;
    }

    /// <summary>
    /// Gets a value indicating whether the service is mocked, spawning no containers. The default
    /// is the emulator's and differs per service.
    /// </summary>
    public bool Mocked { get; init; }

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => !Mocked;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCKED"] = Mocked ? "true" : "false";
    }
}
