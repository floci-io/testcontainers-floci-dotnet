using Floci.Testcontainers.Core;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Aws;

/// <summary>
/// The AWS emulator (<c>floci/floci</c>) as a <see cref="CloudDescriptor" />.
/// </summary>
[PublicAPI]
public static class AwsCloud
{
    /// <summary>
    /// Gets the AWS emulator's descriptor. Its socket services are the configs that override
    /// <c>RequiresDockerAccess</c>; a unit test keeps the two in step.
    /// </summary>
    public static CloudDescriptor Descriptor { get; } = new(
        Name: "aws",
        Port: 4566,
        EnvPrefix: "FLOCI_",
        HealthPath: "/_floci/health",
        ResetPath: "/_floci/state/reset",
        SocketServices: new[]
        {
            new SocketService("BATCH", ModeSetting: "RUNNER_MODE", InProcessMode: "immediate"),
            new SocketService("CODEBUILD"),
            new SocketService("EC2", Mockable: true),
            new SocketService("ECR"),
            new SocketService("ECS", Mockable: true),
            new SocketService("EKS", Mockable: true),
            new SocketService("ELASTICACHE"),
            new SocketService("LAMBDA"),
            new SocketService("MSK", Mockable: true),
            new SocketService("NEPTUNE"),
            new SocketService("OPENSEARCH", Mockable: true),
            new SocketService("RDS"),
        });
}
