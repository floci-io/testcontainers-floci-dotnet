using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Step Functions emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithStepFunctions(StepFunctionsConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithStepFunctions(new StepFunctionsConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record StepFunctionsConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "STEPFUNCTIONS";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Step Functions has no additional settings beyond the enabled flag.
    }
}
