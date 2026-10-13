using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Cosmos DB emulation.
/// </summary>
/// <remarks>
/// The MongoDB, PostgreSQL, Cassandra and Gremlin APIs run their engine in a sibling container, so
/// Cosmos DB needs the Docker socket while one of them is enabled, unless it is mocked or
/// <see cref="EngineStartup" /> is <c>disabled</c>. All four are disabled by default.
/// <code>
/// await using var floci = new FlociAzBuilder("floci/floci-az:latest")
///     .WithCosmos(new CosmosConfig { MongoDb = true })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CosmosConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosConfig" /> class.
    /// </summary>
    public CosmosConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <summary>
    /// Gets when engine containers start: <c>on-demand</c> (the default) or <c>disabled</c>.
    /// </summary>
    public string EngineStartup { get; init; } = "on-demand";

    /// <summary>
    /// Gets a value indicating whether the MongoDB API engine is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool MongoDb { get; init; }

    /// <summary>
    /// Gets a value indicating whether the PostgreSQL API engine is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool PostgreSql { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Cassandra API engine is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool Cassandra { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Gremlin API engine is enabled. Defaults to <see langword="false" />.
    /// </summary>
    public bool Gremlin { get; init; }

    /// <inheritdoc />
    protected override string ServiceKey => "COSMOS";

    /// <inheritdoc />
    internal override bool RequiresDockerAccess =>
        !Mocked
        && !string.Equals(EngineStartup, "disabled", StringComparison.OrdinalIgnoreCase)
        && (MongoDb || PostgreSql || Cassandra || Gremlin);

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        base.AddSettings(env, prefix);
        env[prefix + "ENGINES_STARTUP"] = EngineStartup;
        env[prefix + "ENGINES_MONGODB_ENABLED"] = MongoDb ? "true" : "false";
        env[prefix + "ENGINES_POSTGRESQL_ENABLED"] = PostgreSql ? "true" : "false";
        env[prefix + "ENGINES_CASSANDRA_ENABLED"] = Cassandra ? "true" : "false";
        env[prefix + "ENGINES_GREMLIN_ENABLED"] = Gremlin ? "true" : "false";
    }
}
