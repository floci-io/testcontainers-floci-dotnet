using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Cost Explorer emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCostExplorer(CostExplorerConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithCostExplorer(new CostExplorerConfig { CreditUsdMonthly = 100.0 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CostExplorerConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets the synthetic monthly USD credit applied as a <c>Credit</c> record type row in
    /// <c>GetCostAndUsage</c> responses. Defaults to <c>0.0</c>.
    /// </summary>
    public double CreditUsdMonthly { get; init; } = 0.0;

    /// <inheritdoc />
    protected override string ServiceKey => "CE";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "CREDIT_USD_MONTHLY"] = CreditUsdMonthly.ToString(CultureInfo.InvariantCulture);
    }
}
