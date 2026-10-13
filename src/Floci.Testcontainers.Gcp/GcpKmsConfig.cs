using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud KMS emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithKms(GcpKmsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record GcpKmsConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "KMS";
}
