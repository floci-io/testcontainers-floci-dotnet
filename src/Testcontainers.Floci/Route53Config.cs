using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's Route 53 emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithRoute53(Route53Config)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithRoute53(new Route53Config())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record Route53Config : FlociServiceConfig
{
    /// <summary>Gets the first default nameserver assigned to new hosted zones. Defaults to <c>ns-1.awsdns-01.org</c>.</summary>
    public string DefaultNameserver1 { get; init; } = "ns-1.awsdns-01.org";

    /// <summary>Gets the second default nameserver assigned to new hosted zones. Defaults to <c>ns-2.awsdns-02.net</c>.</summary>
    public string DefaultNameserver2 { get; init; } = "ns-2.awsdns-02.net";

    /// <summary>Gets the third default nameserver assigned to new hosted zones. Defaults to <c>ns-3.awsdns-03.com</c>.</summary>
    public string DefaultNameserver3 { get; init; } = "ns-3.awsdns-03.com";

    /// <summary>Gets the fourth default nameserver assigned to new hosted zones. Defaults to <c>ns-4.awsdns-04.co.uk</c>.</summary>
    public string DefaultNameserver4 { get; init; } = "ns-4.awsdns-04.co.uk";

    /// <inheritdoc />
    protected override string ServiceKey => "ROUTE53";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DEFAULT_NAMESERVER_1"] = DefaultNameserver1;
        env[prefix + "DEFAULT_NAMESERVER_2"] = DefaultNameserver2;
        env[prefix + "DEFAULT_NAMESERVER_3"] = DefaultNameserver3;
        env[prefix + "DEFAULT_NAMESERVER_4"] = DefaultNameserver4;
    }
}
