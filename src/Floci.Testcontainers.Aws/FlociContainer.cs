using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;

namespace Floci.Testcontainers.Aws;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class FlociContainer : DockerContainer
{
    private readonly FlociConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlociContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public FlociContainer(FlociConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the emulator endpoint to point the AWS SDK for .NET at (the <c>ServiceURL</c>).
    /// </summary>
    /// <returns>The endpoint, e.g. <c>http://localhost:32768</c>.</returns>
    public string GetEndpoint()
    {
        return new UriBuilder(Uri.UriSchemeHttp, Hostname, GetMappedPublicPort(FlociBuilder.FlociPort)).ToString();
    }

    /// <summary>
    /// Gets the default AWS region the emulator advertises.
    /// </summary>
    public string Region => _configuration.Region ?? "us-east-1";

    /// <summary>
    /// Gets the default AWS account id the emulator advertises.
    /// </summary>
    public string AccountId => _configuration.AccountId ?? "000000000000";

    /// <summary>
    /// Gets the default availability zone the emulator advertises.
    /// </summary>
    public string AvailabilityZone => _configuration.AvailabilityZone ?? "us-east-1a";

    /// <summary>
    /// Gets the access key for the emulator. Floci accepts any credentials; this is a conventional dummy value.
    /// </summary>
    public string AccessKey => "test";

    /// <summary>
    /// Gets the secret key for the emulator. Floci accepts any credentials; this is a conventional dummy value.
    /// </summary>
    public string SecretKey => "test";

    /// <summary>
    /// Gets the prefix of the sibling containers' names (<c>FLOCI_DOCKER_RESOURCE_NAMESPACE</c>).
    /// </summary>
    public string? ResourceNamespace =>
        _configuration.Environments.TryGetValue(AwsCloud.Descriptor.ResourceNamespaceEnv, out var ns) ? ns : null;

    /// <summary>
    /// Wipes all emulator state (buckets, queues, tables, ...) without restarting the container.
    /// Needs <c>floci/floci</c> 1.5.29 or later.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the state is reset.</returns>
    public Task ResetAsync(CancellationToken ct = default)
    {
        return FlociHost.ResetAsync(AwsCloud.Descriptor, GetEndpoint(), ct);
    }

    /// <summary>
    /// Stops and removes the emulator, then removes the sibling containers it spawned (those
    /// labelled with its <see cref="ResourceNamespace" />). Containers sharing a namespace set
    /// through <see cref="FlociBuilder.WithResourceNamespace" /> lose their siblings too.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
        finally
        {
            var resourceNamespace = ResourceNamespace;
            if (!string.IsNullOrEmpty(resourceNamespace))
            {
                await FlociHost.RemoveSiblingContainersAsync(_configuration.DockerEndpointAuthConfig, resourceNamespace!)
                    .ConfigureAwait(false);
            }
        }
    }
}
