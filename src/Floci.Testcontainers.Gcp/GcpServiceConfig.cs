using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Base type of the Google Cloud emulator's per-service configuration. It emits
/// <c>FLOCI_GCP_SERVICES_&lt;SERVICE&gt;_ENABLED</c>; services with more settings add them.
/// </summary>
[PublicAPI]
public abstract record GcpServiceConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected sealed override string ServicesPrefix => GcpCloud.Descriptor.EnvPrefix + "SERVICES_";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Only the enabled switch, which the base emits.
    }
}
