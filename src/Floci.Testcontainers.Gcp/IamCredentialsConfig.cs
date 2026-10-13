using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's IAM Service Account Credentials emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithIamCredentials(IamCredentialsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record IamCredentialsConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "IAMCREDENTIALS";
}
