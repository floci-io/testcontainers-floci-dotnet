# Testcontainers.Floci (.NET)

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for
[Floci](https://github.com/floci-io/floci) — a free, open-source local AWS emulator.

It spins up the `floci/floci` container for your integration tests and gives you an
endpoint to point the AWS SDK for .NET at. No account, no token.

> **Status:** in development. Core container lifecycle plus typed config for 12 flat services
> (S3, SQS, SNS, DynamoDB, Secrets Manager, SSM, KMS, EventBridge, IAM, Kinesis, CloudWatch
> Logs/Metrics), each with unit + live integration tests. Container-based services (RDS, Lambda,
> ECS, ElastiCache) are not yet implemented.

## Usage

```csharp
await using var floci = new FlociBuilder().Build();
await floci.StartAsync();

using var s3 = new AmazonS3Client(
    floci.AccessKey,
    floci.SecretKey,
    new AmazonS3Config
    {
        ServiceURL = floci.GetEndpoint(),
        ForcePathStyle = true,
        AuthenticationRegion = floci.Region,
    });

await s3.PutBucketAsync("my-bucket");
```

## Building and testing

```bash
dotnet build
dotnet test   # requires a running Docker daemon
```

### Running on Colima

The module itself is environment-agnostic. Colima exposes the Docker socket over virtiofs, so
its macOS host socket path can't be bind-mounted into a container (this otherwise breaks the
Testcontainers resource reaper). Point Testcontainers at the in-VM socket path instead:

```bash
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE="/var/run/docker.sock"
# and, if your DOCKER_HOST isn't already set to the Colima socket:
export DOCKER_HOST="unix://$HOME/.colima/default/docker.sock"
```

On native Linux Docker (e.g. CI), neither variable is needed.

## Relationship to the upstream Floci project

This module mirrors the API of the official
[`testcontainers-floci`](https://github.com/floci-io/testcontainers-floci) (Java) and
[`testcontainers-floci-python`](https://github.com/floci-io/testcontainers-floci-python)
modules. It is intended to be contributed to the `floci-io` organisation.

## License

MIT — see [LICENSE](LICENSE).
