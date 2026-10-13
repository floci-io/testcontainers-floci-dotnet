using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Firestore emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithFirestore(FirestoreConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record FirestoreConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "FIRESTORE";
}
