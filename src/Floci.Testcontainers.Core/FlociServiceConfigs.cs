using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Core;

/// <summary>
/// The per-service configs applied to a builder, at most one per service: applying a config
/// replaces the one applied before for the same service, with everything it had set.
/// </summary>
/// <remarks>
/// A Testcontainers configuration can only add environment variables, never take one back. So
/// the cloud builders do not write a config's variables when it is applied; they keep the config
/// here, and write the variables of the surviving configs when the container is built. A
/// variable the caller set explicitly (<c>WithEnvironment</c>) always wins over a config's,
/// whichever came first.
/// </remarks>
[PublicAPI]
public sealed class FlociServiceConfigs
{
    private readonly IReadOnlyDictionary<string, FlociServiceConfig> _configs;

    private FlociServiceConfigs(IReadOnlyDictionary<string, FlociServiceConfig> configs)
    {
        _configs = configs;
    }

    /// <summary>Gets the empty set.</summary>
    public static FlociServiceConfigs Empty { get; } = new(new Dictionary<string, FlociServiceConfig>());

    /// <summary>
    /// Returns this set with <paramref name="config" /> in the place of the config of the same
    /// service, if there was one.
    /// </summary>
    /// <param name="config">The service configuration.</param>
    /// <returns>The new set.</returns>
    public FlociServiceConfigs With(FlociServiceConfig config)
    {
        return new FlociServiceConfigs(new Dictionary<string, FlociServiceConfig>(Copy(_configs)) { [config.EnvPrefix] = config });
    }

    /// <summary>
    /// Merges two sets the way builder configurations are merged: a service configured in
    /// <paramref name="newValue" /> replaces the same service in <paramref name="oldValue" />.
    /// </summary>
    /// <param name="oldValue">The older set.</param>
    /// <param name="newValue">The newer set.</param>
    /// <returns>The merged set.</returns>
    public static FlociServiceConfigs Combine(FlociServiceConfigs oldValue, FlociServiceConfigs newValue)
    {
        if (newValue._configs.Count == 0)
        {
            return oldValue;
        }

        var merged = Copy(oldValue._configs);
        foreach (var entry in newValue._configs)
        {
            merged[entry.Key] = entry.Value;
        }

        return new FlociServiceConfigs(merged);
    }

    /// <summary>
    /// Returns the container environment these configs lead to: their variables, with
    /// <paramref name="explicitEnvironment" /> on top.
    /// </summary>
    /// <param name="explicitEnvironment">The variables set on the builder directly.</param>
    /// <returns>The effective environment.</returns>
    public IReadOnlyDictionary<string, string> Environment(IReadOnlyDictionary<string, string> explicitEnvironment)
    {
        if (_configs.Count == 0)
        {
            return explicitEnvironment;
        }

        var environment = new Dictionary<string, string>();
        foreach (var config in Ordered())
        {
            foreach (var entry in config.BuildEnvironment())
            {
                environment[entry.Key] = entry.Value;
            }
        }

        foreach (var entry in explicitEnvironment)
        {
            environment[entry.Key] = entry.Value;
        }

        return environment;
    }

    /// <summary>
    /// Gets the host ports the enabled configs need published 1:1.
    /// </summary>
    public IReadOnlyCollection<int> FixedHostPorts =>
        Ordered().Where(config => config.Enabled).SelectMany(config => config.FixedHostPorts).Distinct().ToArray();

    private IEnumerable<FlociServiceConfig> Ordered() =>
        _configs.OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry => entry.Value);

    private static Dictionary<string, FlociServiceConfig> Copy(IReadOnlyDictionary<string, FlociServiceConfig> configs)
    {
        var copy = new Dictionary<string, FlociServiceConfig>();
        foreach (var entry in configs)
        {
            copy[entry.Key] = entry.Value;
        }

        return copy;
    }
}
