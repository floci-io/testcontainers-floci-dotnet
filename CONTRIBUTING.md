# Contributing to Testcontainers.Floci (.NET)

Thanks for your interest in contributing! This explains how to build and test the module, the
branching model, and the conventions to follow.

Please also read the [Code of Conduct](CODE_OF_CONDUCT.md).

## Getting started

### Prerequisites

- **.NET SDK** as pinned in [`global.json`](global.json) (10.0.x).
- **Docker** — required for the integration tests. On native Linux Docker the defaults work as-is.
  On macOS with Colima, see the environment notes in the [README](README.md#building-and-testing).

### Fork and clone

```bash
git clone https://github.com/<your-username>/testcontainers-floci-dotnet.git
cd testcontainers-floci-dotnet
git remote add upstream https://github.com/floci-io/testcontainers-floci-dotnet.git
```

### Build and test

```bash
# Build all targets; the repo must stay warning-clean.
dotnet build Testcontainers.Floci.slnx -c Release -warnaserror

# Unit tier — fast, no Docker.
dotnet test tests/Testcontainers.Floci.Tests/Testcontainers.Floci.Tests.csproj -c Release

# Integration tier — needs Docker; starts a real Floci container per test class.
dotnet test tests/Testcontainers.Floci.IntegrationTests/Testcontainers.Floci.IntegrationTests.csproj -c Release
```

## Project structure

```
src/Testcontainers.Floci/                     Core module (netstandard2.0 .. net10.0)
  FlociConfiguration / FlociBuilder / FlociContainer   The Testcontainers triad
  FlociServiceConfig                          Base record for per-service config
  <Service>Config.cs                          One record per AWS service (SqsConfig is the reference)
tests/Testcontainers.Floci.Tests/             Unit tests (*ConfigTest) — no Docker
tests/Testcontainers.Floci.IntegrationTests/  Integration tests (*ServiceTest) — Docker required
```

## Branching model

- `main` — active development. Open feature branches off `main` and target your PR back at it.

## Adding support for a new Floci service

This is the bulk of the work and follows a fixed template. See the open
[**Add AWS service support**](https://github.com/floci-io/testcontainers-floci-dotnet/issues/new?template=add-service.md)
issues (tracked under the [service-parity issue](https://github.com/floci-io/testcontainers-floci-dotnet/issues/6)).
For each service:

1. **Config record** — `src/Testcontainers.Floci/<Service>Config.cs`, a `sealed record : FlociServiceConfig`
   with one `init` property per setting (same defaults as the Floci/Java reference), `ServiceKey => "<TOKEN>"`,
   and `AddSettings` writing each setting with the exact `FLOCI_SERVICES_<TOKEN>_<SETTING>` env key.
2. **Builder method** — `FlociBuilder.With<Service>(<Service>Config config) => WithServiceConfig(config);`.
3. **Unit test** — `tests/Testcontainers.Floci.Tests/<Service>ConfigTest.cs`: defaults, default env output,
   custom env output, disabled-only.
4. **Integration test** — `tests/Testcontainers.Floci.IntegrationTests/<Service>ServiceTest.cs`: an
   `IAsyncLifetime` that starts a `FlociBuilder().Build()` and does a real round-trip via the AWS SDK client
   pointed at `GetEndpoint()`. Add the `AWSSDK.<Service>` package to the integration project.
5. Container-based services (those that make Floci spawn sibling containers) additionally override
   `RequiresDockerAccess` / `FixedHostPorts` and should delete the resource in teardown.

## Commit messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/). The commit history drives
the version that `publish.yml` ships automatically on every push to `main`.

| Prefix | Version bump | Example |
|---|---|---|
| `fix:` / `perf:` / `refactor:` / `test:` | Patch | `fix: handle null region gracefully` |
| `feat:` | Minor | `feat: add WithOpenSearch configuration` |
| `feat!:` or `BREAKING CHANGE:` | Major | `feat!: rename GetEndpoint to GetServiceUrl` |
| `chore:` / `ci:` / anything else | Patch | `chore: bump AWSSDK.S3` |

Docs/metadata-only pushes (`**.md`, `LICENSE`, `.gitignore`) don't cut a release — see the `paths-ignore`
in [`publish.yml`](.github/workflows/publish.yml).

## Releases

Releases are automated. A push to `main` computes the next version from the commit history, tags it, creates a
GitHub Release, and publishes the package. Contributors don't manage versions or tags.
