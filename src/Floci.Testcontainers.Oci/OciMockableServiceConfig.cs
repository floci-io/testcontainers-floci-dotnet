using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Base type of the OCI services that have a mock switch (<c>…_MOCK</c>): mocked, the
/// service answers without spawning containers, so it does not need the Docker socket.
/// </summary>
[PublicAPI]
public abstract record OciMockableServiceConfig : OciServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether the service is mocked, spawning no containers. Defaults to
    /// <see langword="false" />, as in the emulator.
    /// </summary>
    public bool Mock { get; init; }

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => !Mock;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
    }
}
