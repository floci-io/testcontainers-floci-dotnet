<p align="center">
  <img src="https://raw.githubusercontent.com/floci-io/.github/main/floci.svg#gh-light-mode-only" alt="Floci" width="500" />
  <img src="https://github.com/user-attachments/assets/edfff8b3-926c-471e-9549-77fb90a21b49#gh-dark-mode-only" alt="Floci" width="500" />
</p>

<p align="center">
  <strong>Any Cloud. Locally.</strong><br />
  Light, fluffy, and always free: Testcontainers for .NET<br />
  No account. No auth token. No feature gates.
</p>

<p align="center">
  <a href="https://github.com/floci-io/testcontainers-floci-dotnet/actions/workflows/release-please.yml"><img src="https://github.com/floci-io/testcontainers-floci-dotnet/actions/workflows/release-please.yml/badge.svg" alt="publish"></a>
  <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/license-MIT-green" alt="License: MIT"></a>
  <a href="https://github.com/floci-io/testcontainers-floci-dotnet/stargazers"><img src="https://img.shields.io/github/stars/floci-io/testcontainers-floci-dotnet?style=flat" alt="GitHub Stars"></a>
</p>

<p align="center">
  <a href="#quick-start">Quick Start</a> ·
  <a href="#service-configuration">Configuration</a> ·
  <a href="#the-floci-emulators">Emulators</a> ·
  <a href="https://floci.io/floci/testcontainers/dotnet/">Docs</a>
</p>

---

## What is this?

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for
[Floci](https://github.com/floci-io/floci), the free, open-source local AWS emulator.

It runs the `floci/floci` container for your integration tests, gives you an endpoint to point
the AWS SDK for .NET at, and adds a **typed, per-service configuration API** over Floci's
environment-variable surface, including the container-based services (RDS, Lambda, ElastiCache,
ECS, EC2, ECR and others) that spawn real backing containers. No AWS account, no token.

This module mirrors the API of the official
[`testcontainers-floci`](https://github.com/floci-io/testcontainers-floci) (Java) and
[`testcontainers-floci-python`](https://github.com/floci-io/testcontainers-floci-python) modules.
It adds no emulation logic of its own: Floci does all the AWS emulation, and this is the typed
.NET front door to it.

> Targets `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`.

### The Floci emulators

testcontainers-floci-dotnet is the .NET member of the [Floci](https://github.com/floci-io)
Testcontainers family. Floci is named after
[floccus](https://en.wikipedia.org/wiki/Cirrocumulus_floccus), the cloud formation that looks like
popcorn.

| Emulator                                           | Cloud | Port | Supported                                       |
|----------------------------------------------------|-------|:----:|:-----------------------------------------------:|
| [floci](https://github.com/floci-io/floci)         | AWS   | 4566 | ✅ [`Testcontainers.Floci`](#installation)       |
| [floci-az](https://github.com/floci-io/floci-az)   | Azure | 4577 | Planned                                         |
| [floci-gcp](https://github.com/floci-io/floci-gcp) | GCP   | 4588 | Planned                                         |
| [floci-oci](https://github.com/floci-io/floci-oci) | OCI   | 4599 | Planned                                         |

## Installation

```bash
dotnet add package Testcontainers.Floci
```

The package is published to the floci-io GitHub Packages feed (not nuget.org). See
[Consuming from GitHub Packages](#consuming-from-github-packages) for the one-time `nuget.config`
setup required.

### Consuming from GitHub Packages

The package id is `Testcontainers.Floci`, the same id as the minimal official package on
nuget.org, so consumers **must** use NuGet
[package source mapping](https://learn.microsoft.com/nuget/consume-packages/package-source-mapping)
to route that id to the floci-io feed. Add a `nuget.config` to the consuming repo:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-floci-io" value="https://nuget.pkg.github.com/floci-io/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-floci-io>
      <add key="Username" value="%GITHUB_ACTOR%" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_PAT%" />
    </github-floci-io>
  </packageSourceCredentials>
  <packageSourceMapping>
    <!-- Route ONLY our package to the floci-io feed; everything else to nuget.org. -->
    <packageSource key="github-floci-io">
      <package pattern="Testcontainers.Floci" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

`GITHUB_PACKAGES_PAT` is a PAT with `read:packages`. **The `packageSourceMapping` block is
required**: without it, NuGet may resolve `Testcontainers.Floci` from nuget.org (the unrelated,
minimal official package) instead of ours.

### Versioning

Releases are auto-versioned from [Conventional Commits](https://www.conventionalcommits.org/) on
`main`: `feat:` → minor, `fix:`/`perf:`/`refactor:`/`test:` → patch, `!`/`BREAKING CHANGE` → major.
Other commits still ship a patch. Write conventional commit messages for meaningful version bumps.

## Quick start

Use it from an xUnit test via `IAsyncLifetime`. The builder takes the image, optional global
settings (region/account), and per-service config; `GetEndpoint()` plus the dummy credentials wire
up any AWS SDK client:

```csharp
public sealed class OrderTests : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder("floci/floci:latest")
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

## Service configuration

| Method                              | Description                                                         |
|-------------------------------------|---------------------------------------------------------------------|
| `new FlociBuilder(string image)`    | Creates a builder for the given image (see [Docker image tags](#docker-image-tags)) |
| `new FlociBuilder(IImage image)`    | Same, from a Testcontainers `IImage`                                |
| `WithRegion(string)`                | Sets the AWS region (default: `us-east-1`)                          |
| `WithAvailabilityZone(string)`      | Sets the default availability zone (default: `us-east-1a`)          |
| `WithAccountId(string)`             | Sets the default AWS account ID (default: `000000000000`)           |
| `WithXxx(XxxConfig)`                | Configures service-specific settings                                |

Each service has a typed `XxxConfig` record exposing its settings (mirroring Floci's
`FLOCI_SERVICES_*` env vars), applied via `builder.WithXxx(...)`. Enabling a service is just
adding its `WithXxx`; Floci enables most services by default regardless. See the
[Floci documentation](https://floci.io/floci/services/) for the full list of services the
emulator supports.

### Supported services

**Flat services** (single endpoint), for example S3, SQS, SNS, DynamoDB, Secrets Manager, SSM,
KMS, EventBridge (plus Pipes and Scheduler), IAM, Kinesis, Firehose, CloudWatch Logs and Metrics,
SES and SES v2, Step Functions, Glue, Cognito, CloudFormation, API Gateway (v1 & v2), Route 53,
CloudFront, ELBv2, Athena, AppConfig, AppSync, Textract, Transcribe, AWS Config, Auto Scaling,
Backup, Transfer Family, CloudTrail, Cloud Map, EMR, WAFv2, RDS Data, and the billing APIs
(Cost Explorer, CUR, Pricing, BCM Data Exports).

**Container-based services** (spawn real backing containers): RDS (Postgres/MySQL/MariaDB),
ElastiCache (Valkey/Redis & Memcached), Lambda (real function execution), ECS, EC2, ECR, MSK
(Redpanda), OpenSearch, Neptune (Gremlin), EKS (k3s), CodeBuild, Batch.

STS works out of the box (always on; no config). The authoritative list of config records is the
set of `*Config.cs` files in [`src/Testcontainers.Floci`](src/Testcontainers.Floci).

### Container-based services

Container-based services make Floci spawn **sibling containers** (a real Postgres, a Lambda
runtime, a Valkey, a Redpanda broker, an OpenSearch node, a k3s cluster, etc.). The module mounts
the Docker socket (only while such a service is enabled) and publishes the needed ports
automatically. A few practical notes when using them:

- Connect to spawned backends via `127.0.0.1` (not `localhost`) to avoid IPv6 resolution.
- On macOS, RDS's default proxy base port `7000` collides with Control Center (AirPlay); set
  `new RdsConfig { ProxyBasePort = 7010 }`.
- Delete resources you create in test teardown (`DeleteDBInstance`, `DeleteReplicationGroup`,
  `DeleteFunction`) so Floci removes their sibling containers. They are managed by Floci, not
  tracked by the Testcontainers resource reaper.
- ECS and EC2 support a `Mock` mode (`new EcsConfig { Mock = true }`) that returns RUNNING without
  launching containers, handy for deterministic tests.

## Container options

| Member                | Description                                                        | Default        |
|-----------------------|--------------------------------------------------------------------|----------------|
| `GetEndpoint()`       | HTTP endpoint URL to use as `ServiceURL` (e.g. `http://localhost:32768`) | —        |
| `Region`              | Configured AWS region                                              | `us-east-1`    |
| `AvailabilityZone`    | Configured default availability zone                               | `us-east-1a`   |
| `AccountId`           | Configured default AWS account ID                                  | `000000000000` |
| `AccessKey`           | AWS access key                                                     | `test`         |
| `SecretKey`           | AWS secret key                                                     | `test`         |

Readiness is determined by polling Floci's `/_floci/health` endpoint on port 4566, which is
mapped to a random host port.

## Docker image tags

**Always pass the image explicitly**, e.g. `new FlociBuilder("floci/floci:latest")`. Every Floci
emulator publishes `latest`, `x.y.z` and `nightly` tags, so you can pin a release, follow `latest`,
or test against `main` with `nightly`:

```csharp
new FlociBuilder("floci/floci:x.y.z");   // a specific release (recommended for CI)
new FlociBuilder("floci/floci:latest");  // the current emulator
new FlociBuilder("floci/floci:nightly"); // built from main every night
```

This module deliberately diverges from the rest of the family here. The other modules default to
the floating `latest` tag; the .NET builder's baked-in default is a **pinned** tag
(a specific `x.y.z` release), exposed through the parameterless `FlociBuilder()` constructor and the
`FlociBuilder.FlociImage` constant. Both are marked `[Obsolete]` and will be removed in a future
version in favour of always passing the image, so new code should not rely on them.

The tag this repository's integration tests run against is declared in
[`TestImages.cs`](tests/Testcontainers.Floci.IntegrationTests/TestImages.cs). The images cached
by CI are listed in [`.github/docker-images.txt`](.github/docker-images.txt).

## Requirements

- A target framework supported by the package (`net8.0`+ or any `netstandard2.0`/`2.1` consumer)
- Docker

## Building and testing

```bash
dotnet build -warnaserror
dotnet test   # requires a running Docker daemon
```

Unit tests (`*ConfigTest`, no Docker) live in
[`tests/Testcontainers.Floci.Tests`](tests/Testcontainers.Floci.Tests) and integration tests
(`*ServiceTest`, real Floci containers) in
[`tests/Testcontainers.Floci.IntegrationTests`](tests/Testcontainers.Floci.IntegrationTests).
Test parallelization is disabled in the integration project on purpose: container-backed tests
flake under Docker daemon load. See [CONTRIBUTING.md](CONTRIBUTING.md) for the project layout,
the branching model, and how to add a service.

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

## Other languages

| Language | Repository |
|---|---|
| Java | [testcontainers-floci](https://github.com/floci-io/testcontainers-floci) |
| Node.js / TypeScript | [testcontainers-floci-node](https://github.com/floci-io/testcontainers-floci-node) |
| Python | [testcontainers-floci-python](https://github.com/floci-io/testcontainers-floci-python) |
| Go | [testcontainers-floci-go](https://github.com/floci-io/testcontainers-floci-go) |
| .NET | **testcontainers-floci-dotnet** (this repo) |

## Community

- 💬 [Slack](https://join.slack.com/t/floci/shared_invite/zt-3tjn02s3q-A00kEjJ1cZxsg_imTfy6Cw): quick questions and community chat
- 🗣️ [GitHub Discussions](https://github.com/orgs/floci-io/discussions): ideas, design tradeoffs, and proposals
- [CONTRIBUTING.md](CONTRIBUTING.md) · [SECURITY.md](SECURITY.md) · [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) · [MAINTAINERS.md](MAINTAINERS.md)

## License

MIT. See [LICENSE](LICENSE).

---

<div align="center">

Floci™ is a trademark of Hector Ventura. Code is MIT-licensed; see
[TRADEMARK.md](https://github.com/floci-io/.github/blob/main/TRADEMARK.md) for name and logo use.

</div>
