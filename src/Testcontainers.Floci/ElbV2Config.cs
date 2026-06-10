using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's ELBv2 (Elastic Load Balancing v2) emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithElbV2(ElbV2Config)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithElbV2(new ElbV2Config { Mock = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record ElbV2Config : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether load balancers are simulated without starting a real load
    /// balancer process. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; } = false;

    /// <summary>
    /// Gets the listener ports to be published 1:1 (host == container port) so load-balancer
    /// traffic is reachable from the host. Only applied when the service is enabled.
    /// </summary>
    public IReadOnlyList<int> ListenerPorts { get; init; } = Array.Empty<int>();

    /// <inheritdoc />
    protected override string ServiceKey => "ELBV2";

    /// <inheritdoc />
    internal override IReadOnlyCollection<int> FixedHostPorts => ListenerPorts;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "MOCK"] = Mock ? "true" : "false";
    }
}
