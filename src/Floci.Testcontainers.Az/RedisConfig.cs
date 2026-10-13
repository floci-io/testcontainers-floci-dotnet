using JetBrains.Annotations;

namespace Floci.Testcontainers.Az;

/// <summary>
/// Configuration for Floci's Azure Cache for Redis emulation.
/// </summary>
/// <remarks>
/// Unless mocked, the service runs each cache as a sibling container. It is not mocked by default, so it needs the Docker socket unless mocked.
/// Apply via <see cref="FlociAzBuilder.WithRedis(RedisConfig)" />.
/// </remarks>
[PublicAPI]
public sealed record RedisConfig : AzMockableServiceConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConfig" /> class.
    /// </summary>
    public RedisConfig()
        : base(mockedByDefault: false)
    {
    }

    /// <inheritdoc />
    protected override string ServiceKey => "REDIS";
}
