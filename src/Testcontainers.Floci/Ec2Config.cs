using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's EC2 emulation.
/// </summary>
/// <remarks>
/// In real mode (the default) EC2 instances are backed by Docker containers, so the Docker socket
/// is mounted; in mock mode instances go straight to RUNNING with no container. The IMDS
/// (instance metadata) port is published to the host.
/// </remarks>
[PublicAPI]
public sealed record Ec2Config : FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether instances go straight to RUNNING without launching Docker
    /// containers. Defaults to <see langword="false" />.
    /// </summary>
    public bool Mock { get; init; }

    /// <summary>
    /// Gets the host port for the IMDS (instance metadata) HTTP server. Defaults to <c>9169</c>.
    /// </summary>
    public int ImdsPort { get; init; } = 9169;

    /// <summary>
    /// Gets the lowest host port published for instance SSH access. Defaults to <c>2200</c>.
    /// </summary>
    public int SshPortRangeStart { get; init; } = 2200;

    /// <summary>
    /// Gets the highest host port published for instance SSH access. Defaults to <c>2299</c>.
    /// </summary>
    public int SshPortRangeEnd { get; init; } = 2299;

    /// <summary>
    /// Gets a value indicating whether EC2 Auto Scaling is enabled. Defaults to <see langword="true" />.
    /// </summary>
    public bool AutoScalingEnabled { get; init; } = true;

    /// <inheritdoc />
    protected override string ServiceKey => "EC2";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => !Mock;

    /// <inheritdoc />
    internal override IReadOnlyCollection<int> FixedHostPorts => new[] { ImdsPort };

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // Floci namespaces the Auto Scaling toggle separately (FLOCI_SERVICES_AUTOSCALING_*), not
        // under EC2. Upstream emits it unconditionally; we emit it whenever EC2 is enabled, which
        // is the only case that matters in practice.
        env["FLOCI_SERVICES_AUTOSCALING_ENABLED"] = AutoScalingEnabled ? "true" : "false";
        env[prefix + "MOCK"] = Mock ? "true" : "false";
        env[prefix + "IMDS_PORT"] = ImdsPort.ToString(CultureInfo.InvariantCulture);
        env[prefix + "SSH_PORT_RANGE_START"] = SshPortRangeStart.ToString(CultureInfo.InvariantCulture);
        env[prefix + "SSH_PORT_RANGE_END"] = SshPortRangeEnd.ToString(CultureInfo.InvariantCulture);
    }
}
