using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Secret Manager emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithSecretManager(SecretManagerConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record SecretManagerConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "SECRETMANAGER";
}
