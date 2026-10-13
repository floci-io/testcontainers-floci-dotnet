using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Core;

/// <summary>
/// Base type for Floci per-service configuration.
/// </summary>
/// <remarks>
/// Mirrors the upstream Java module's <c>AbstractServiceConfig</c>. Each service is a small
/// record exposing typed settings; the base turns them into the <c>FLOCI_SERVICES_&lt;SERVICE&gt;_*</c>
/// environment variables Floci reads. The <c>ENABLED</c> key is always emitted; the remaining
/// settings are emitted only when the service is enabled.
/// </remarks>
[PublicAPI]
public abstract record FlociServiceConfig
{
    /// <summary>
    /// Gets a value indicating whether the service is enabled. Defaults to <see langword="true" />.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the SCREAMING_SNAKE service token used in the env-var prefix (e.g. <c>SQS</c>).
    /// </summary>
    protected abstract string ServiceKey { get; }

    /// <summary>
    /// Gets the env-var prefix of the cloud's services, including the trailing underscore.
    /// Defaults to the AWS emulator's <c>FLOCI_SERVICES_</c>; other clouds override it
    /// (e.g. <c>FLOCI_AZ_SERVICES_</c>).
    /// </summary>
    protected virtual string ServicesPrefix => "FLOCI_SERVICES_";

    /// <summary>
    /// Gets the service token (<see cref="ServiceKey" />), for the module's own consistency checks.
    /// </summary>
    internal string ServiceToken => ServiceKey;

    /// <summary>
    /// Gets the prefix of every env var of the service, e.g. <c>FLOCI_SERVICES_SQS_</c>. It
    /// identifies the service among the configs applied to a builder.
    /// </summary>
    internal string EnvPrefix => ServicesPrefix + ServiceKey + "_";

    /// <summary>
    /// Adds the service-specific settings to <paramref name="env" />. Only called when the
    /// service is enabled. <paramref name="prefix" /> already includes the trailing underscore
    /// (e.g. <c>FLOCI_SERVICES_SQS_</c>).
    /// </summary>
    /// <param name="env">The environment-variable map to populate.</param>
    /// <param name="prefix">The fully-qualified env-var prefix for this service.</param>
    protected abstract void AddSettings(IDictionary<string, string> env, string prefix);

    /// <summary>
    /// Gets a value indicating whether this service spawns sibling containers and therefore
    /// needs the host Docker socket mounted into the Floci container. Container-based services
    /// (RDS, Lambda, ECS, ElastiCache) override this to return <see langword="true" />.
    /// </summary>
    internal virtual bool RequiresDockerAccess => false;

    /// <summary>
    /// Gets the host ports that must be published 1:1 (host port == container port) so the host
    /// can reach sibling containers — e.g. the RDS proxy range, where Floci returns
    /// <c>endpoint=localhost:&lt;port&gt;</c> literally. Only applied when the service is enabled.
    /// </summary>
    internal virtual IReadOnlyCollection<int> FixedHostPorts => Array.Empty<int>();

    /// <summary>
    /// Builds the environment variables that represent this configuration.
    /// </summary>
    /// <returns>The environment variables to apply to the container.</returns>
    internal IReadOnlyDictionary<string, string> BuildEnvironment()
    {
        var prefix = EnvPrefix;

        var env = new Dictionary<string, string>
        {
            [prefix + "ENABLED"] = Enabled ? "true" : "false",
        };

        if (Enabled)
        {
            AddSettings(env, prefix);
        }

        return env;
    }
}
