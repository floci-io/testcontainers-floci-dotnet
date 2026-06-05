using System.Collections.Generic;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's DynamoDB emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithDynamoDb(DynamoDbConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder()
///     .WithDynamoDb(new DynamoDbConfig())
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record DynamoDbConfig : FlociServiceConfig
{
    /// <inheritdoc />
    protected override string ServiceKey => "DYNAMODB";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        // DynamoDB has no additional settings beyond the enabled flag.
    }
}
