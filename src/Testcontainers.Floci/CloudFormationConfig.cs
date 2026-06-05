using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

/// <summary>
/// Configuration for Floci's CloudFormation emulation.
/// </summary>
/// <remarks>
/// Apply via <see cref="FlociBuilder.WithCloudFormation(CloudFormationConfig)" />:
/// <code>
/// await using var floci = new FlociBuilder(TestImages.Floci)
///     .WithCloudFormation(new CloudFormationConfig { DeletedStackRetentionSeconds = 60 })
///     .Build();
/// </code>
/// </remarks>
[PublicAPI]
public sealed record CloudFormationConfig : FlociServiceConfig
{
    /// <summary>
    /// Gets how long deleted stacks are retained in seconds. Defaults to <c>30</c>.
    /// </summary>
    public long DeletedStackRetentionSeconds { get; init; } = 30L;

    /// <inheritdoc />
    protected override string ServiceKey => "CLOUDFORMATION";

    /// <inheritdoc />
    protected override void AddSettings(IDictionary<string, string> env, string prefix)
    {
        env[prefix + "DELETED_STACK_RETENTION_SECONDS"] = DeletedStackRetentionSeconds.ToString(CultureInfo.InvariantCulture);
    }
}
