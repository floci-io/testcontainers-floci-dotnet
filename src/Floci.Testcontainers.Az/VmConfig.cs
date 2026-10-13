using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Virtual Machines emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each virtual machine as a container. It is mocked by default, so it needs the Docker socket only when the mock is switched off.
/// Apply via <see cref="FlociAzBuilder.WithVm(VmConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record VmConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VmConfig" /> class.
    /// </summary>
    public VmConfig()
        : base(mockedByDefault: true)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "VM";
}
