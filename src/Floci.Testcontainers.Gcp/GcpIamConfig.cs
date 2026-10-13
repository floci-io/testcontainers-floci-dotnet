using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's IAM emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithIam(GcpIamConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GcpIamConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "IAM";
}
