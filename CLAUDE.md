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
export DOCKER_HOST="unix:///Users/james/.colima/default/docker.sock"
```

- `dotnet build` — must stay clean (0 warnings).
- `dotnet test --filter "FullyQualifiedName~XxxConfigTest"` — fast unit tier, no Docker.
- `dotnet test` — full suite, needs Docker (Colima). Ryuk is auto-disabled (see gotchas).

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

- **Ryuk on Colima**: the resource reaper can't bind-mount the virtiofs Docker socket.
  `TestcontainersSetup.cs` (a `[ModuleInitializer]`) sets `TESTCONTAINERS_RYUK_DISABLED=true`
  unless already set. Don't remove it. CI with a real daemon can re-enable by setting it `false`.
- **`IsExternalInit`**: required for `init` setters on netstandard2.0. We ship our own polyfill
  (`src/Testcontainers.Floci/IsExternalInit.cs`) so the modreq doesn't bind to a transitive
  assembly (which breaks net consumers). Keep it.
- **Container-based services** (RDS, Lambda, ECS, ElastiCache) need the Docker socket mounted +
  root, per the Java `FlociContainer`. Not yet implemented here — these need core work beyond the
  flat-service template before their configs are useful. Defer them.

## Git

Commit at green checkpoints. **No `Co-Authored-By` trailers.**
