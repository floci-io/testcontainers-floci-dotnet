using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Firebase Authentication (Identity Platform) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithFirebaseAuth(FirebaseAuthConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record FirebaseAuthConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "FIREBASEAUTH";
}
