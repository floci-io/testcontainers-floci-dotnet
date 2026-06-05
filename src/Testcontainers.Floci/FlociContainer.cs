using System;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;

namespace Testcontainers.Floci;

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
}
