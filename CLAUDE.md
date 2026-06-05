# CLAUDE.md — Testcontainers.Floci (.NET)

A Testcontainers for .NET module for [Floci](https://github.com/floci-io/floci), a free local
AWS emulator. Built to replace LocalStack in CI after LocalStack's Community edition closed.
Intended to be contributed upstream to the `floci-io` org.

## Layout

- `src/Testcontainers.Floci/` — the module (netstandard2.0 library)
  - `FlociConfiguration` / `FlociBuilder` / `FlociContainer` — the Testcontainers triad (core)
  - `FlociServiceConfig` — base record for per-service config
  - `SqsConfig` — the **reference implementation** of the per-service template
- `tests/Testcontainers.Floci.Tests/` — xUnit tests (net10.0)
- Local read-only references (not in this repo): `~/repos/_reference/testcontainers-floci`
  (Java) and `~/repos/_reference/testcontainers-floci-python`. Port from these.

## Build & test

All `dotnet` commands need these in the environment:

```bash
export PATH="/usr/local/share/dotnet:$PATH"
# Colima only — see "Running on Colima" below. Not needed on native Linux Docker.
export DOCKER_HOST="unix:///Users/james/.colima/default/docker.sock"
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE="/var/run/docker.sock"
```

- `dotnet build` — must stay clean (0 warnings).
- `dotnet test --filter "FullyQualifiedName~XxxConfigTest"` — fast unit tier, no Docker.
- `dotnet test` — full suite, needs Docker.

### Running on Colima

The committed code is environment-agnostic — no Colima specifics. Colima exposes the Docker
socket over virtiofs, so its **macOS host** socket path can't be bind-mounted into a container
(this breaks Ryuk and any socket mount). The fix is purely environmental: set
`TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock` so Testcontainers mounts the
**in-VM** socket path instead. With that set, Ryuk works normally — do **not** disable it.
On native Linux Docker (e.g. CI) neither var is needed; the defaults are correct.

## The per-service template (how to add a service)

This is the pattern. Replicating it across services is the bulk of the work and is suited to
Sonnet. For each AWS service, read the Java `XxxConfig.java` + its two tests, then:

1. **Config record** — `src/Testcontainers.Floci/XxxConfig.cs`, a `sealed record : FlociServiceConfig`:
   - One `init` property per setting, with the **same defaults as the Java class**.
   - `ServiceKey => "XXX"` (the SCREAMING_SNAKE token in the env prefix).
   - `AddSettings` writes each setting using the **exact Java env-var key** (the part after
     `FLOCI_SERVICES_XXX_`). Use `CultureInfo.InvariantCulture` for numbers; `"true"`/`"false"` for bools.
2. **Builder method** — add a one-line `public FlociBuilder WithXxx(XxxConfig config) => WithServiceConfig(config);`
   to `FlociBuilder.cs`.
3. **Unit test** — `tests/.../XxxConfigTest.cs`: defaults, default env output, custom env output,
   disabled-only case. Mirrors the Java `XxxConfigTest`. Uses the internal `BuildEnvironment()`
   (exposed via `InternalsVisibleTo`).
4. **Integration test** — `tests/.../XxxServiceTest.cs`: `IAsyncLifetime` starting a
   `FlociBuilder().Build()`, an `AmazonXxxClient` pointed at `GetEndpoint()` with
   `AuthenticationRegion = _floci.Region` and dummy `AccessKey`/`SecretKey`, doing a real
   round-trip. Mirrors the Java `XxxServiceTest`. Add the `AWSSDK.Xxx` package to the test project.

Keep each integration test small (1–2 round-trips). Commit per service (or per small batch)
once `dotnet test` is green.

## Conventions & decisions (do not relitigate)

- **Records with `init` properties**, not Java-style inner Builders. Idiomatic C#, low boilerplate.
- **Naming: `XxxConfig`** (matches upstream; avoids clashing with `FlociConfiguration`).
- **Env scheme: `FLOCI_SERVICES_<SERVICE>_<SETTING>`**; `ENABLED` always emitted, other settings
  only when enabled. Top-level: `FLOCI_DEFAULT_REGION` / `FLOCI_DEFAULT_ACCOUNT_ID` /
  `FLOCI_DEFAULT_AVAILABILITY_ZONE`. Verified against the running image — match Java keys exactly.
- Endpoint is `GetEndpoint()` (`http://host:mappedPort/`); credentials are dummy `test`/`test`.
- Image: `floci/floci:latest`, port 4566, native arm64.

## Gotchas

- **Ryuk on Colima**: handled by environment, not code (see "Running on Colima"). Set
  `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock` and Ryuk works. Do not add any
  Colima-specific code (e.g. a ModuleInitializer that disables Ryuk) — keep the module portable.
- **`IsExternalInit`**: required for `init` setters on netstandard2.0. We ship our own polyfill
  (`src/Testcontainers.Floci/IsExternalInit.cs`) so the modreq doesn't bind to a transitive
  assembly (which breaks net consumers). Keep it.
- **Lambda** (done, container-based): runs each function's real AWS Lambda runtime container,
  pulled from `public.ecr.aws/lambda/*` on first invoke (python3.12 ≈ 778 MB, cached after). Real
  execution (handler output reflects input). `python3.12` and `nodejs20.x` confirmed; handler
  `lambda_function.handler`; the role ARN is accepted but not validated; `State` is `Active`
  synchronously. Invocations go through the gateway so Runtime API ports need no publishing
  (`ExposeRuntimePorts` default false). First invoke is a cold start (~8–10s); set a generous
  client timeout. `DeleteFunction` removes the runtime container (call it in teardown).
  `AwsConfigPath` is supported (emits `FLOCI_SERVICES_LAMBDA_AWS_CONFIG_PATH`); Floci mounts that
  host path into the function container — on Colima it must be a VM-visible path, not a macOS one.
- **Container-based services** (RDS + Lambda done; ECS/ElastiCache pending): Floci spawns sibling
  containers via the Docker daemon. A config opts in by overriding `RequiresDockerAccess` (mounts
  `/var/run/docker.sock`) and `FixedHostPorts` (publishes ports **1:1**, since Floci returns
  `endpoint=localhost:<port>` literally). `FlociBuilder.WithServiceConfig` honours both. Hard-won
  gotchas from RDS (see `RdsServiceTest`):
  - **macOS port 7000 collision**: Control Center / AirPlay Receiver listens on `*:7000`, so the
    default RDS `ProxyBasePort = 7000` is intercepted by the OS on a Mac. Tests use `7010`.
  - **Connect via `127.0.0.1`, not `localhost`** — avoids Npgsql resolving to IPv6 (`::1`).
  - **`SSL Mode=Disable`** — Floci's RdsAuthProxy doesn't do SSL negotiation.
  - **Siblings leak**: Floci-spawned DB containers are Floci-managed (not Ryuk-tracked) and are
    named after the instance id, so a leak collides on re-run. Call `DeleteDBInstance` in test
    teardown so Floci removes them.
- **Test parallelization is disabled** (`AssemblyInfo.cs`, `DisableTestParallelization = true`):
  container-backed tests starting many Floci containers at once flake under Docker-daemon load.

## Git

Commit at green checkpoints. **No `Co-Authored-By` trailers.**
