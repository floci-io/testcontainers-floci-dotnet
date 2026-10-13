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
[Floci](https://github.com/floci-io/floci), the free, open-source local AWS emulator, and for
[Floci Azure](https://github.com/floci-io/floci-az) and [Floci GCP](https://github.com/floci-io/floci-gcp),
its Azure and Google Cloud counterparts (see [Azure](#azure) and [GCP](#gcp)).

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
| [floci](https://github.com/floci-io/floci)         | AWS   | 4566 | ✅ [`Floci.Testcontainers.Aws`](#installation)   |
| [floci-az](https://github.com/floci-io/floci-az)   | Azure | 4577 | ✅ [`Floci.Testcontainers.Az`](#azure)           |
| [floci-gcp](https://github.com/floci-io/floci-gcp) | GCP   | 4588 | ✅ [`Floci.Testcontainers.Gcp`](#gcp)            |
| [floci-oci](https://github.com/floci-io/floci-oci) | OCI   | 4599 | Planned                                         |

## Installation

```bash
dotnet add package Floci.Testcontainers.Aws
```

`Floci.Testcontainers.Aws` brings in `Floci.Testcontainers.Core`, the shared part every Floci cloud
package builds on. Code lives in the `Floci.Testcontainers.Aws` namespace.

For Azure, add `Floci.Testcontainers.Az` (namespace `Floci.Testcontainers.Az`); for Google Cloud,
`Floci.Testcontainers.Gcp` (namespace `Floci.Testcontainers.Gcp`). Their types carry an `Az` or `Gcp`
infix (`FlociAzBuilder`, `FlociGcpContainer`), so one test project can reference several clouds.

The packages are published to [nuget.org](https://www.nuget.org/packages/Floci.Testcontainers.Aws), so no
feed or token setup is needed.

### Migrating from `Testcontainers.Floci`

Up to 0.16.x the package was `Testcontainers.Floci`, an id that belongs to the official
testcontainers-dotnet module on nuget.org. From 1.0.0 it is `Floci.Testcontainers.Aws`:

1. Replace the package reference: `dotnet remove package Testcontainers.Floci`, then
   `dotnet add package Floci.Testcontainers.Aws`. The 0.16.x line was published to the floci-io GitHub
   Packages feed; 1.0.0 and later come from nuget.org, so the floci-io feed, its credentials and its
   `packageSourceMapping` entry can be removed from `nuget.config`. Without that mapping, NuGet could also
   resolve `Testcontainers.Floci` from nuget.org, where that id is the unrelated official module.
2. Replace `using Testcontainers.Floci;` with `using Floci.Testcontainers.Aws;`.

The types (`FlociBuilder`, `FlociContainer`, every `*Config`) keep their names and behaviour, with one
difference: the Docker socket is now decided from the container's final environment, so it is
mounted by default (see [Container-based services](#container-based-services)).

### Versioning

Releases are cut with [release-please](https://github.com/googleapis/release-please), as in the Java
module. Each [Conventional Commit](https://www.conventionalcommits.org/) merged to `main` updates one
release PR (`feat:` → minor, `fix:`/`perf:` → patch, `!`/`BREAKING CHANGE` → major). A release happens
when a maintainer merges that PR.

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

### Azure

`FlociAzBuilder` starts [Floci Azure](https://github.com/floci-io/floci-az) (`floci/floci-az`) and
`FlociAzContainer` gives the Azure SDK for .NET what it needs to connect:

```csharp
await using var floci = new FlociAzBuilder("floci/floci-az:latest").Build();
await floci.StartAsync();

var blobs = new BlobServiceClient(floci.GetStorageConnectionString());
var container = (await blobs.CreateBlobContainerAsync("orders")).Value;
await container.GetBlobClient("order-1.txt").UploadAsync(BinaryData.FromString("hello from floci"));
```

| Member                         | Description                                                             | Default                                  |
|--------------------------------|-------------------------------------------------------------------------|------------------------------------------|
| `GetEndpoint()`                | HTTP base URL of every service (e.g. `http://localhost:32781`)          | —                                        |
| `GetHttpsEndpoint()`           | HTTPS base URL, served on the same port once `WithTls()` is set         | —                                        |
| `GetTlsCertificateAsync()`     | PEM certificate HTTPS clients have to trust (needs `WithTls()`)         | —                                        |
| `SubscriptionId`               | Default subscription                                                    | `00000000-0000-0000-0000-000000000001`   |
| `TenantId`                     | Default Microsoft Entra ID tenant                                       | `00000000-0000-0000-0000-000000000002`   |
| `AccountName` / `AccountKey`   | Default storage account and its key                                     | `devstoreaccount1`, the development key  |
| `GetBlobEndpoint()`            | Blob Storage endpoint (`http://127.0.0.1:<port>/devstoreaccount1`)      | —                                        |
| `GetQueueEndpoint()`           | Queue Storage endpoint (`…/devstoreaccount1-queue`)                     | —                                        |
| `GetTableEndpoint()`           | Table Storage endpoint (`…/devstoreaccount1-table`)                     | —                                        |
| `GetStorageConnectionString()` | Connection string with the Blob, Queue and Table endpoints              | —                                        |
| `ResourceNamespace`            | Prefix of the sibling containers' names                                 | `tc-` + 8 hex                            |
| `ResetAsync()`                 | Wipes all emulator state without restarting                             | —                                        |

The storage endpoints, and so the connection string, address the emulator by IP (`127.0.0.1` rather
than `localhost`): the Azure Storage SDK for .NET reads the account name from the URL path only when
the host is an IP address. With `WithTls()`, `StartAsync` also waits until the emulator answers
over HTTPS, which is slightly later than over HTTP, so HTTPS clients can connect straight away.

Every Azure service has a config record with an `Enabled` switch, applied with `WithXxx(...)`
(`WithBlob(new BlobConfig { Enabled = false })`). Services that can run without containers also have a
`Mocked` switch whose default is the emulator's: Service Bus, Container Instances, Virtual Machines and
Container Apps are mocked by default, while for example AKS, Redis, PostgreSQL and Functions run real
sibling containers unless mocked. `CosmosConfig` and `SqlConfig` also carry the settings that decide
whether they spawn containers. Other settings go through `WithEnvironment("FLOCI_AZ_SERVICES_…", …)`
for now. The authoritative list of config records is the set of `*Config.cs` files in
[`src/Floci.Testcontainers.Az`](https://github.com/floci-io/testcontainers-floci-dotnet/tree/main/src/Floci.Testcontainers.Az).

The Docker socket, the resource namespace and sibling cleanup work as described under
[Container-based services](#container-based-services). Other builder options: `WithTls()`,
`WithLogLevel(level)`, `WithResourceNamespace`, `WithDockerSocket(path)` and `WithoutDockerSocket()`.
The builder always sets `FLOCI_AZ_DOCKER_ENDPOINT_MODE=published` (so the addresses the emulator
reports for its sibling containers are reachable from the test) and `FLOCI_AZ_TLS_HTTPS_PORT=0`;
`WithEnvironment` overrides either. Readiness polls `/_floci/health` on port 4577.

### GCP

`FlociGcpBuilder` starts [Floci GCP](https://github.com/floci-io/floci-gcp) (`floci/floci-gcp`) and
`FlociGcpContainer` gives the Google Cloud client libraries what they need to connect:

```csharp
await using var floci = new FlociGcpBuilder("floci/floci-gcp:latest").Build();
await floci.StartAsync();

var storage = await new StorageClientBuilder
{
    BaseUri = floci.GetEndpoint() + "/storage/v1/",
    UnauthenticatedAccess = true,
}.BuildAsync();
await storage.CreateBucketAsync(floci.ProjectId, "orders");
```

The libraries can also find the emulator on their own, through the `*_EMULATOR_HOST` variables.
`GetEmulatorEnv()` returns them for this container, to set on the process under test:

```csharp
foreach (var variable in floci.GetEmulatorEnv())
{
    Environment.SetEnvironmentVariable(variable.Key, variable.Value);
}

var publisher = await new PublisherServiceApiClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOnly,
}.BuildAsync();
```

| Member                     | Description                                                              | Default        |
|----------------------------|--------------------------------------------------------------------------|----------------|
| `GetEndpoint()`            | HTTP base URL of every service (e.g. `http://localhost:32781`)           | —              |
| `GetHostAndPort()`         | The same address without a scheme, as gRPC clients take it               | —              |
| `ProjectId`                | Default project; any project id is accepted                              | `floci-local`  |
| `GetEmulatorEnv()`         | `PUBSUB_`, `FIRESTORE_`, `DATASTORE_`, `SECRET_MANAGER_`, `FIREBASE_AUTH_`, `LOGGING_` and `KMS_EMULATOR_HOST`, `STORAGE_EMULATOR_HOST`, `STORAGE_EMULATOR_HOST_GRPC`, `GOOGLE_CLOUD_PROJECT` | — |
| `GetHttpsEndpoint()`       | HTTPS base URL, served on the same port once `WithTls()` is set          | —              |
| `GetTlsCertificateAsync()` | PEM certificate HTTPS clients have to trust (needs `WithTls()`)          | —              |
| `ResourceNamespace`        | Prefix of the sibling containers' names                                  | `tc-` + 8 hex  |
| `ResetAsync()`             | Wipes all emulator state without restarting                              | —              |

Every `*_EMULATOR_HOST` value is `host:port`, except `STORAGE_EMULATOR_HOST`, which the Cloud Storage
libraries expect as a URL. `LOGGING_EMULATOR_HOST` and `KMS_EMULATOR_HOST` are a floci-gcp convention
that no Google client library reads; they are there for your own code to read the endpoint from.
gRPC is served in plain text on the same port as REST. Services without
such a variable (BigQuery, Cloud Tasks, Cloud Monitoring and others) take `GetEndpoint()` as an
explicit endpoint.

Every Google Cloud service has a config record with an `Enabled` switch, applied with `WithXxx(...)`
(`WithPubSub(new PubSubConfig { Enabled = false })`). BigQuery, Cloud Run, Cloud SQL, GKE and Kafka run
real sibling containers and also have a `Mock` switch that turns that off
(`WithGke(new GkeConfig { Mock = true })`). Other settings go through
`WithEnvironment("FLOCI_GCP_SERVICES_…", …)` for now. Three records carry a `Gcp` prefix because the
AWS package has a type of the same name: `GcpIamConfig`, `GcpKmsConfig` and `GcpSchedulerConfig`
(their methods are `WithIam`, `WithKms` and `WithScheduler`). The authoritative list of config records
is the set of `*Config.cs` files in
[`src/Floci.Testcontainers.Gcp`](https://github.com/floci-io/testcontainers-floci-dotnet/tree/main/src/Floci.Testcontainers.Gcp).

The Docker socket, the resource namespace and sibling cleanup work as described under
[Container-based services](#container-based-services). Other builder options: `WithProjectId(id)`,
`WithTls()`, `WithLogLevel(level)`, `WithResourceNamespace`, `WithDockerSocket(path)` and
`WithoutDockerSocket()`. The builder always sets `FLOCI_GCP_TLS_HTTPS_PORT=0`; `WithEnvironment`
overrides it. Readiness polls `/_floci-gcp/health` on port 4588, over HTTPS as well once `WithTls()` is
set, so HTTPS clients can connect as soon as `StartAsync` returns.

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

Applying a config for a service again replaces the earlier one whole: nothing the first one set
stays behind, in every cloud package. A variable you set yourself with `WithEnvironment` is kept and
wins over the same variable from a config, whichever was applied first.

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
set of `*Config.cs` files in [`src/Floci.Testcontainers.Aws`](https://github.com/floci-io/testcontainers-floci-dotnet/tree/main/src/Floci.Testcontainers.Aws).

### Container-based services

Container-based services make Floci spawn **sibling containers** (a real Postgres, a Lambda
runtime, a Valkey, a Redpanda broker, an OpenSearch node, a k3s cluster, etc.). The module mounts
the Docker socket when an enabled service needs it and publishes the needed ports automatically.
The decision reads the container's final environment, including raw `WithEnvironment` overrides.
Floci enables every service by default, so the socket is mounted unless those services are disabled
or mocked; `WithoutDockerSocket()` and `WithDockerSocket(path)` force the choice either way. Each
container also gets a unique resource namespace (`tc-xxxxxxxx`) that prefixes the sibling containers'
names, so parallel test runs never collide. A few practical notes when using them:

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
| `ResourceNamespace`   | Prefix of the sibling containers' names                            | `tc-` + 8 hex  |
| `ResetAsync()`        | Wipes all emulator state without restarting (Floci 1.5.29+)        | —              |

Builder options beyond the per-service configs: `WithRegion`, `WithAccountId`, `WithAvailabilityZone`,
`WithResourceNamespace`, `WithDockerSocket(path)` and `WithoutDockerSocket()`.

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
`FlociAzBuilder` and `FlociGcpBuilder` have no default image at all: pass it, e.g.
`new FlociAzBuilder("floci/floci-az:latest")` or `new FlociGcpBuilder("floci/floci-gcp:latest")`.

The tag this repository's integration tests run against is declared in
[`TestImages.cs`](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/tests/Floci.Testcontainers.Aws.IntegrationTests/TestImages.cs)
(Azure: [`TestImages.cs`](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/tests/Floci.Testcontainers.Az.IntegrationTests/TestImages.cs),
GCP: [`TestImages.cs`](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/tests/Floci.Testcontainers.Gcp.IntegrationTests/TestImages.cs)). The images cached
by CI are listed in [`.github/docker-images.txt`](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/.github/docker-images.txt).

## Requirements

- A target framework supported by the package (`net8.0`+ or any `netstandard2.0`/`2.1` consumer)
- Docker

## Building and testing

```bash
dotnet build -warnaserror
dotnet test   # requires a running Docker daemon
```

Unit tests (`*ConfigTest`, no Docker) live in
[`tests/Floci.Testcontainers.Aws.Tests`](https://github.com/floci-io/testcontainers-floci-dotnet/tree/main/tests/Floci.Testcontainers.Aws.Tests) and integration tests
(`*ServiceTest`, real Floci containers) in
[`tests/Floci.Testcontainers.Aws.IntegrationTests`](https://github.com/floci-io/testcontainers-floci-dotnet/tree/main/tests/Floci.Testcontainers.Aws.IntegrationTests);
the Azure and GCP ones are in the `tests/Floci.Testcontainers.Az.*` and `tests/Floci.Testcontainers.Gcp.*` projects.
Test parallelization is disabled in the integration projects on purpose: container-backed tests
flake under Docker daemon load. See [CONTRIBUTING.md](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/CONTRIBUTING.md) for the project layout,
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
- [CONTRIBUTING.md](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/CONTRIBUTING.md) · [SECURITY.md](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/SECURITY.md) · [CODE_OF_CONDUCT.md](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/CODE_OF_CONDUCT.md) · [MAINTAINERS.md](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/MAINTAINERS.md)

## License

MIT. See [LICENSE](https://github.com/floci-io/testcontainers-floci-dotnet/blob/main/LICENSE).

---

<div align="center">

Floci™ is a trademark of Hector Ventura. Code is MIT-licensed; see
[TRADEMARK.md](https://github.com/floci-io/.github/blob/main/TRADEMARK.md) for name and logo use.

</div>
