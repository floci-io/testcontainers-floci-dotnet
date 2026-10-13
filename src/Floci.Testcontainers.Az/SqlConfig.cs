using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Data-plane providers of the Azure SQL servers.
/// </summary>
[PublicAPI]
public enum SqlDataPlaneProvider
{
    /// <summary>Control plane only: servers and databases exist as ARM resources without a live endpoint.</summary>
    None,

    /// <summary>Each server is backed by a SQL Server container spawned by Floci Azure.</summary>
    Managed,

    /// <summary>Servers point at an externally provided SQL Server.</summary>
    External,
}

/// <summary>
/// Configuration for Floci's Azure SQL Database emulation.
/// </summary>
/// <remarks>
/// With the <see cref="SqlDataPlaneProvider.Managed" /> data plane each server runs SQL Server in a
/// sibling container, which needs the Docker socket. That is the effective provider once the SQL
/// Server EULA is accepted:
/// <code>
/// await using var floci = new FlociAzBuilder("floci/floci-az:latest")
///     .WithSql(new SqlConfig { AcceptEula = "Y" })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record SqlConfig : AzServiceConfig
{
    /// <summary>
    /// Gets the data-plane provider, or <see langword="null" /> (the default) to let the emulator
    /// derive it from <see cref="AcceptEula" />.
    /// </summary>
    public SqlDataPlaneProvider? DataPlaneProvider { get; init; }

    /// <summary>
    /// Gets whether the SQL Server EULA is accepted: <c>Y</c> or <c>N</c> (the default).
    /// </summary>
    public string AcceptEula { get; init; } = "N";

    /// <summary>
    /// Gets the provider the emulator effectively uses: <see cref="DataPlaneProvider" /> when set,
    /// otherwise <see cref="SqlDataPlaneProvider.Managed" /> if the EULA is accepted and
    /// <see cref="SqlDataPlaneProvider.None" /> if not.
    /// </summary>
    public SqlDataPlaneProvider EffectiveDataPlaneProvider =>
        DataPlaneProvider
        ?? (string.Equals(AcceptEula, "Y", StringComparison.OrdinalIgnoreCase) ? SqlDataPlaneProvider.Managed : SqlDataPlaneProvider.None);

    /// <inheritdoc />
    protected override string ServiceKey => "SQL";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess => EffectiveDataPlaneProvider == SqlDataPlaneProvider.Managed;

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "ACCEPT_EULA"] = AcceptEula;

        if (DataPlaneProvider is { } provider)
        {
            env[prefix + "DATA_PLANE_PROVIDER"] = provider.ToString().ToLowerInvariant();
        }
    }
}
