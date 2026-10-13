using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Functions emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs functions in sibling containers. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithFunctions(FunctionsConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record FunctionsConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionsConfig" /> class.
    /// </summary>
    public FunctionsConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "FUNCTIONS";
}
