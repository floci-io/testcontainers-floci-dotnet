using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Eventarc emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithEventarc(EventarcConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record EventarcConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "EVENTARC";
}
