using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Datastore emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithDatastore(DatastoreConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record DatastoreConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "DATASTORE";
}
