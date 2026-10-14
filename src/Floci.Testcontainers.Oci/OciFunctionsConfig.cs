using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Configuration for Floci's Functions emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs invocations on an Fn Project server sibling container, so it needs the Docker socket unless <see cref="OciMockableServiceConfig.Mock" /> is set.
/// Apply via <see cref="FlociOciBuilder.WithFunctions(OciFunctionsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record OciFunctionsConfig : OciMockableServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "FUNCTIONS";
}
