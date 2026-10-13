using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Base type of the Azure emulator's per-service configuration. It emits
/// <c>FLOCI_AZ_SERVICES_&lt;SERVICE&gt;_ENABLED</c>; services with more settings add them.
/// </summary>
[PublicAPI]
public abstract record AzServiceConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected sealed override string ServicesPrefix => AzCloud.Descriptor.EnvPrefix + "SERVICES_";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Only the enabled switch, which the base emits.
    }
}
