# Testcontainers.Floci (.NET)

[![publish](https://github.com/FinLegal/testcontainers-floci-dotnet/actions/workflows/publish.yml/badge.svg)](https://github.com/FinLegal/testcontainers-floci-dotnet/actions/workflows/publish.yml)

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for
[Floci](https://github.com/floci-io/floci) — a free, open-source local AWS emulator.

It runs the `floci/floci` container for your integration tests, gives you an endpoint to point
the AWS SDK for .NET at, and adds a **typed, per-service configuration API** over Floci's
environment-variable surface — including the container-based services (RDS, Lambda, ElastiCache,
ECS, EC2, ECR) that spawn real backing containers. No AWS account, no token.

> Targets `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`.

## Install

```bash
dotnet add package Testcontainers.Floci
```

The package is published to the FinLegal GitHub Packages feed (not nuget.org) — see
[Consuming from GitHub Packages](#consuming-from-github-packages) for the one-time `nuget.config`
setup required.

## Quick start

Use it from an xUnit test via `IAsyncLifetime`. The builder takes the image, optional global
settings (region/account), and per-service config; `GetEndpoint()` + the dummy credentials wire
up any AWS SDK client:

```csharp
public sealed class OrderTests : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder("floci/floci:1.5.22")
        .WithRegion("eu-west-2")
        .WithSqs(new SqsConfig { VisibilityTimeout = 60 })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();
    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    [Fact]
    public async Task SendsAndReceives()
    {
        using var sqs = new AmazonSQSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSQSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });

        var queueUrl = (await sqs.CreateQueueAsync("orders")).QueueUrl;
        await sqs.SendMessageAsync(queueUrl, "hello from floci");

        var received = await sqs.ReceiveMessageAsync(queueUrl);
        Assert.Single(received.Messages);
    }
}
```

Each service has a typed `XxxConfig` record exposing its settings (mirroring Floci's
`FLOCI_SERVICES_*` env vars), applied via `builder.WithXxx(...)`. Enabling a service is just
adding its `WithXxx`; Floci enables most services by default regardless.

### Container-based services

RDS, Lambda, ElastiCache, ECS, EC2 and ECR make Floci spawn **sibling containers** (a real
Postgres, a Lambda runtime, a Valkey, etc.). The module mounts the Docker socket and publishes the
needed ports automatically. A few practical notes when using them:

- Connect to spawned backends via `127.0.0.1` (not `localhost`) to avoid IPv6 resolution.
- On macOS, RDS's default proxy base port `7000` collides with Control Center (AirPlay) — set
  `new RdsConfig { ProxyBasePort = 7010 }`.
- Delete resources you create in test teardown (`DeleteDBInstance`, `DeleteReplicationGroup`,
  `DeleteFunction`) so Floci removes their sibling containers.
- ECS and EC2 support a `Mock` mode (`new EcsConfig { Mock = true }`) that returns RUNNING without
  launching containers — handy for deterministic tests.

## Supported services

**Flat services** (single endpoint): S3, SQS, SNS, DynamoDB, Secrets Manager, SSM, KMS,
EventBridge, IAM, Kinesis, CloudWatch Logs, CloudWatch Metrics, SES, Step Functions, Glue,
Cognito, CloudFormation, API Gateway (v1 & v2), Resource Groups Tagging, Firehose.

**Container-based services** (spawn real backing containers): RDS (Postgres/MySQL/MariaDB),
ElastiCache (Valkey/Redis & Memcached), Lambda (real function execution), ECS, EC2, ECR.

STS works out of the box (always on; no config). Every service has both fast config unit tests and
a live integration test.

## Consuming from GitHub Packages

The package id is `Testcontainers.Floci` — the same id as the minimal official package on
nuget.org — so consumers **must** use NuGet
[package source mapping](https://learn.microsoft.com/nuget/consume-packages/package-source-mapping)
to route that id to the FinLegal feed. Add a `nuget.config` to the consuming repo:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-finlegal" value="https://nuget.pkg.github.com/FinLegal/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-finlegal>
      <add key="Username" value="%GITHUB_ACTOR%" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_PAT%" />
    </github-finlegal>
  </packageSourceCredentials>
  <packageSourceMapping>
    <!-- Route ONLY our package to the FinLegal feed; everything else to nuget.org. -->
    <packageSource key="github-finlegal">
      <package pattern="Testcontainers.Floci" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

`GITHUB_PACKAGES_PAT` is a PAT with `read:packages`. **The `packageSourceMapping` block is
required** — without it, NuGet may resolve `Testcontainers.Floci` from nuget.org (the unrelated,
minimal official package) instead of ours.

## Building and testing

```bash
dotnet build
dotnet test   # requires a running Docker daemon
```

### Running on Colima

The module itself is environment-agnostic. Colima exposes the Docker socket over virtiofs, so its
macOS host socket path can't be bind-mounted into a container (this otherwise breaks the
Testcontainers resource reaper). Point Testcontainers at the in-VM socket path instead:

```bash
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE="/var/run/docker.sock"
# and, if your DOCKER_HOST isn't already set to the Colima socket:
export DOCKER_HOST="unix://$HOME/.colima/default/docker.sock"
```

On native Linux Docker (e.g. CI), neither variable is needed.

## Versioning

Releases are auto-versioned from [Conventional Commits](https://www.conventionalcommits.org/) on
`main`: `feat:` → minor, `fix:`/`perf:`/`refactor:`/`test:` → patch, `!`/`BREAKING CHANGE` → major.
Other commits still ship a patch. Write conventional commit messages for meaningful version bumps.

## Relationship to the upstream Floci project

This module mirrors the API of the official
[`testcontainers-floci`](https://github.com/floci-io/testcontainers-floci) (Java) and
[`testcontainers-floci-python`](https://github.com/floci-io/testcontainers-floci-python) modules,
and is intended to be contributed to the `floci-io` organisation. It adds no emulation logic of
its own — Floci does all the AWS emulation; this is the typed .NET front door to it.

## License

MIT — see [LICENSE](LICENSE).
