using JetBrains.Annotations;

namespace Floci.Testcontainers.Gcp;

/// <summary>
/// Configuration for Floci's Cloud Tasks emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociGcpBuilder.WithCloudTasks(CloudTasksConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record CloudTasksConfig : GcpServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDTASKS";
}
