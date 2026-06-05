# Testcontainers.Floci (.NET)

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for
[Floci](https://github.com/floci-io/floci) — a free, open-source local AWS emulator.

It spins up the `floci/floci` container for your integration tests and gives you an
endpoint to point the AWS SDK for .NET at. No account, no token.

> **Status:** in development. Core container lifecycle plus typed config for 25 AWS services, each
> with unit + live integration tests. Includes container-based services that spawn real backing
> containers — **RDS** (Postgres/MySQL/MariaDB), **ElastiCache** (Valkey/Redis), **Lambda** (real
> function execution), **ECS**, **EC2**, **ECR** — alongside flat services (S3, SQS, SNS, DynamoDB,
> Secrets Manager, SSM, KMS, EventBridge, IAM, Kinesis, CloudWatch Logs/Metrics, SES, Step
> Functions, Glue, Cognito, CloudFormation, API Gateway v1/v2, Resource Groups Tagging, Firehose).

## Usage

```csharp
await using var floci = new FlociBuilder("floci/floci:1.5.22").Build();
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

## Consuming from GitHub Packages

The package is published to the FinLegal GitHub Packages feed on every push to `main`
(auto-versioned from Conventional Commits). The package id is `Testcontainers.Floci` — the same
id as the minimal official package on nuget.org — so consumers **must** use NuGet
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

## Versioning

Releases are auto-versioned from [Conventional Commits](https://www.conventionalcommits.org/) on
`main`: `feat:` → minor, `fix:`/`perf:`/`refactor:`/`test:` → patch, `!`/`BREAKING CHANGE` → major.
Other commits still ship a patch. Write conventional commit messages for meaningful version bumps.

## Relationship to the upstream Floci project

This module mirrors the API of the official
[`testcontainers-floci`](https://github.com/floci-io/testcontainers-floci) (Java) and
[`testcontainers-floci-python`](https://github.com/floci-io/testcontainers-floci-python)
modules. It is intended to be contributed to the `floci-io` organisation.

## License

MIT — see [LICENSE](LICENSE).
