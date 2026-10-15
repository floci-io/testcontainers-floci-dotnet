# CLAUDE.md — Floci.Testcontainers (.NET)

A Testcontainers for .NET module for [Floci](https://github.com/floci-io/floci), a free local
AWS emulator. Built to replace LocalStack in CI after LocalStack's Community edition closed.
Intended to be contributed upstream to the `floci-io` org.

## Layout

- `src/Floci.Testcontainers.Core/` — the cloud-independent core (package `Floci.Testcontainers.Core`)
  - `CloudDescriptor` / `SocketService` — one emulator's facts as data (port, env prefix, health and
    reset paths, the services that need the Docker socket)
  - `FlociHost` — host Docker socket path and state reset
  - `FlociServiceConfig` — base record for per-service config
- `src/Floci.Testcontainers.Aws/` — the AWS module (package `Floci.Testcontainers.Aws`)
  - `AwsCloud.Descriptor` — the AWS descriptor
  - `FlociConfiguration` / `FlociBuilder` / `FlociContainer` — the Testcontainers triad
  - `SqsConfig` — the **reference implementation** of the per-service template
- `tests/Floci.Testcontainers.Aws.Tests/` — xUnit unit tests (net10.0), incl. `CoreTest`
- `tests/Floci.Testcontainers.Aws.IntegrationTests/` — Docker-backed `*ServiceTest`
- `src/Floci.Testcontainers.Az/` — the Azure module (package `Floci.Testcontainers.Az`), see "Azure" below
- `tests/Floci.Testcontainers.Az.Tests/` and `tests/Floci.Testcontainers.Az.IntegrationTests/` — its tests
- `src/Floci.Testcontainers.Gcp/` — the Google Cloud module (package `Floci.Testcontainers.Gcp`), see "GCP" below
- `tests/Floci.Testcontainers.Gcp.Tests/` and `tests/Floci.Testcontainers.Gcp.IntegrationTests/` — its tests
- `src/Floci.Testcontainers.Oci/` — the OCI module (package `Floci.Testcontainers.Oci`), see "OCI" below
- `tests/Floci.Testcontainers.Oci.Tests/` and `tests/Floci.Testcontainers.Oci.IntegrationTests/` — its tests
  The old id `Testcontainers.Floci` belongs to the official testcontainers-dotnet module on nuget.org,
  hence the `Floci.` prefix.
- Local read-only reference (not in this repo): the upstream Java module at
  `~/repos/testcontainers-floci`. Port from it (read `XxxConfig.java` + its tests per service).

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

1. **Config record** — `src/Floci.Testcontainers.Aws/XxxConfig.cs`, a `sealed record : FlociServiceConfig`:
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

## Azure (`Floci.Testcontainers.Az`)

Starts `floci/floci-az` (port 4577, prefix `FLOCI_AZ_`, health `/_floci/health`, reset
`POST /_admin/reset`). Same triad with an `Az` infix (`FlociAzConfiguration` / `FlociAzBuilder` /
`FlociAzContainer`) so a test project can reference both clouds; `AzCloud` holds the descriptor and
the identity defaults. The Java reference is `testcontainers-floci/testcontainers-floci-az`; the
service list and tokens come from `ServicesConfig` in `floci-az`'s `EmulatorConfig.java`.

- **Scope is the cloud MVP**: every service has a record with `Enabled`, and `Mocked` where the
  emulator has a mock switch (`AzMockableServiceConfig`; the key is `…_MOCKED`, not AWS's `…_MOCK`,
  and the default is per service: ACI, Container Apps, Service Bus and VM are mocked by default).
  Typed settings beyond that are ported from Java service by service. The only ones present are those
  socket detection needs: `CosmosConfig` (engine startup and the four Docker-backed API engines) and
  `SqlConfig` (data-plane provider, EULA).
- **Socket table**: `AzCloud.Descriptor.SocketServices`, using the core's `MockSetting`,
  `MockedByDefault` and `Condition`. `AzCloudTest` keeps it in step with the configs.
- **No baked-in image**: `FlociAzBuilder` only has constructors that take the image.
- **Always set, overridable**: `FLOCI_AZ_DOCKER_ENDPOINT_MODE=published`, `FLOCI_AZ_TLS_HTTPS_PORT=0`.
- **`GetEndpoint()` has no trailing slash** (unlike the AWS container's), because the storage
  endpoints and the connection string are built from it exactly as in Java.
- **Storage endpoints use an IP host** (`localhost` becomes `127.0.0.1`, other names are resolved):
  the Azure Storage SDK for .NET treats the first path segment as the account only for an IP host;
  with `localhost` it drops the container or queue name. `GetEndpoint()` keeps the host name.
- **TLS readiness**: with TLS enabled the emulator's port-sniffing proxy accepts connections, and
  plain HTTP is healthy, about 200-330 ms before the HTTPS backend listens (its log shows `TLS proxy:
  failed to connect to backend port 4571: Connection refused`); a handshake in that window ends in an
  EOF, whatever the ALPN. So `Build()` adds an HTTPS wait on the health path when
  `FLOCI_AZ_TLS_ENABLED=true` is in the final environment (`WithTlsReadiness`, backed by the core's
  `FlociReadiness.WithHttpsWhenTlsEnabled`; the probe does not validate the certificate). It is added
  to the configured wait strategies, so a caller's `WithWaitStrategy` is kept. Tests must not retry
  around it.
- **Deliberate divergences from Java**: `AppConfigurationConfig` / `WithAppConfiguration` (Java:
  `AppConfigConfig`), since `Floci.Testcontainers.Aws` already has an `AppConfigConfig` and both
  namespaces are meant to be imported together; and `PolicyConfig` (`FLOCI_AZ_SERVICES_POLICY_ENABLED`),
  which the emulator has and Java does not yet.
- Image: tests use `TestImages.FlociAz` (`floci/floci-az:0.15.0`); keep it in sync with
  `.github/docker-images.txt`.

## GCP (`Floci.Testcontainers.Gcp`)

Starts `floci/floci-gcp` (port 4588, prefix `FLOCI_GCP_`, health `/_floci-gcp/health`, reset
`POST /_floci-gcp/state/reset`). Triad with a `Gcp` infix (`FlociGcpConfiguration` / `FlociGcpBuilder` /
`FlociGcpContainer`); `GcpCloud` holds the descriptor and the default project. **There is no Java
reference**: the source of truth is `ServicesConfig` in `floci-gcp`'s `EmulatorConfig.java` and its
`docs/`, read at the pinned tag.

- **Scope is the cloud MVP**: every service has a record with `Enabled`, and `Mock` where the emulator
  has a mock switch (`GcpMockableServiceConfig`; the key is `…_MOCK`, default `false`). No other typed
  settings yet.
- **Socket table**: `GcpCloud.Descriptor.SocketServices` is BigQuery, Cloud Run, Cloud SQL, GKE and
  Kafka, the five mockable services. BigQuery has a `Condition`: with
  `FLOCI_GCP_SERVICES_BIGQUERY_DUCK_URL` set it uses that sidecar and spawns nothing. Cloud Functions
  is metadata only (no runtime), so it is not in the table. `GcpCloudTest` keeps table and configs in step.
- **No baked-in image**: `FlociGcpBuilder` only has constructors that take the image.
- **TLS readiness**: as in Azure, `Build()` adds the core's HTTPS wait when `FLOCI_GCP_TLS_ENABLED=true`
  is in the final environment (the same proxy race exists here; the backend port is 4581).
- **Always set, overridable**: `FLOCI_GCP_TLS_HTTPS_PORT=0`. `FLOCI_GCP_SERVICES_GKE_ENDPOINT_MODE` is
  left at the emulator's default `host`, which advertises `localhost:<published port>`; `network` would
  advertise the k3s container's name, which only resolves inside the Docker network.
- **Connection helpers**: `GetEndpoint()` (`http://host:port`, no trailing slash), `GetHostAndPort()`
  (gRPC is served in plain text on the same port), `ProjectId` (`WithProjectId` sets
  `FLOCI_GCP_DEFAULT_PROJECT_ID`) and `GetEmulatorEnv()`. In that map every `*_EMULATOR_HOST` is
  `host:port` except `STORAGE_EMULATOR_HOST`, which is a URL. The emulator itself reads none of these;
  they are for the Google client libraries. No library reads `LOGGING_EMULATOR_HOST` or
  `KMS_EMULATOR_HOST` (floci-gcp's compatibility tests do); they are kept for the family contract.
- **Type names that clash with the AWS package carry the cloud prefix**: `GcpIamConfig`,
  `GcpKmsConfig`, `GcpSchedulerConfig` (AWS has `IamConfig`, `KmsConfig`, `SchedulerConfig`, and the
  namespaces are meant to be imported together). The builder methods stay `WithIam`, `WithKms`,
  `WithScheduler`.
- **Integration tests set process environment variables**: `FlociGcpFixture` applies
  `GetEmulatorEnv()` to the test process so clients are built with emulator detection, which is only
  safe because the suite runs serially.
- Image: tests use `TestImages.FlociGcp` (`floci/floci-gcp:0.11.0`); keep it in sync with
  `.github/docker-images.txt`.

## OCI (`Floci.Testcontainers.Oci`)

Starts `floci/floci-oci` (port 4599, prefix `FLOCI_OCI_`, health `/_floci-oci/health`, reset
`POST /_floci-oci/state/reset`). Triad with an `Oci` infix; `OciCloud` holds the descriptor and the
identity defaults. **There is no Java reference**: the source of truth is `ServicesConfig` in
`floci-oci`'s `EmulatorConfig.java` and its `docs/`, read at the pinned tag.

- **Scope is the cloud MVP**: every service has a record with `Enabled`; Functions and OKE, the two
  that spawn containers, also have `Mock` (`OciMockableServiceConfig`, key `…_MOCK`, default `false`)
  and are the whole socket table. No other typed settings yet.
- **No baked-in image**; **always set, overridable**: `FLOCI_OCI_TLS_HTTPS_PORT=0`.
- **Credentials**: the OCI SDKs and CLI refuse to run without an API key, while the emulator parses
  the signature's `keyId` (`tenancy/user/fingerprint`) and never verifies it. So the container hands
  out `UserId` and `Fingerprint` (the fixed placeholders of the emulator's docs), `TenancyId`,
  `Region`, and `GetPrivateKeyPem()`: a 2048-bit RSA key generated lazily, once per container object.
  `GetConfigFileContents(keyFilePath, profile)` renders an `~/.oci/config` profile from them; the
  caller writes the key and the config to disk. The fingerprint is **not** that of the key.
  `WithPrivateKeyPem` replaces the generated key (kept in `FlociOciConfiguration.PrivateKeyPem`).
- **`RsaKeyPem` encodes PKCS#1 DER by hand** because `netstandard2.0` has no key export. `RsaKeyPemTest`
  checks it byte for byte against the platform's own export.
- **The tenancy is the partition**: resources are kept per tenancy taken from the signature, and
  unsigned requests use `FLOCI_OCI_DEFAULT_TENANCY_ID`. `WithTenancyId` sets that default and the
  tenancy of the handed-out credentials together, so both see the same resources. Since floci-oci
  0.5.0 each tenancy also has its own Object Storage namespace (the default tenancy keeps
  `Namespace`, others get one derived from their OCID) and a path namespace that is not the caller's
  answers 404 `NamespaceNotFound`: a client signing as another tenancy asks `GetNamespace` for its own.
- **`FLOCI_OCI_AUTH_REQUIRE_SIGNATURE=true` cannot be used with this container on 0.4.2**: the emulator
  then answers 401 on its own exempt paths (`/health`, `/_floci-oci/*`), so the readiness wait never
  succeeds. It is an emulator bug; do not work around it here. That is why no test enables it.
- **OCI SDK for .NET gotcha**: `client.SetEndpoint(...)` alone is not enough. The client built from a
  region keeps that region's endpoint template and prefers it per request, so `ObjectStorageClient`
  148.1.0 called Oracle's real endpoint (401 "The required information to complete authentication was
  not provided"). Follow it with `client.SetRealmSpecificEndpointTemplate(null)`, as `OciSdk` in the
  integration tests does.
- **No TLS helpers**: floci-oci 0.4.2 can serve HTTPS (`FLOCI_OCI_TLS_ENABLED`) but has no endpoint to
  fetch its self-signed certificate from (floci-az and floci-gcp have `/…/tls-cert`), so there is no
  `WithTls` / `GetHttpsEndpoint` here yet. `Build()` still adds the core's HTTPS wait when
  `FLOCI_OCI_TLS_ENABLED=true` is set through `WithEnvironment`; `TlsServiceTest` covers that.
- **Type names that clash with another cloud package carry the cloud prefix**: `OciFunctionsConfig`,
  `OciKmsConfig`, `OciQueueConfig`. The builder methods stay `WithFunctions`, `WithKms`, `WithQueue`.
- Image: tests use `TestImages.FlociOci` (`floci/floci-oci:0.5.0`); keep it in sync with
  `.github/docker-images.txt`.

## Conventions & decisions (do not relitigate)

- **Records with `init` properties**, not Java-style inner Builders. Idiomatic C#, low boilerplate.
- **Naming: `XxxConfig`** (matches upstream; avoids clashing with `FlociConfiguration`).
- **Service configs are kept, not written, until `Build()`**: `WithXxx(config)` stores the config in
  the configuration's `ServiceConfigs` (core `FlociServiceConfigs`, one per service, the later one
  replaces the earlier one whole). A Testcontainers configuration can only add environment variables
  and port bindings, so writing them at `WithXxx` time left a replaced config's settings and fixed
  ports behind. `Build()` calls `WithServiceSettings()` first, which writes the surviving configs'
  variables and ports. A variable set with `WithEnvironment` wins over a config's **whichever came
  first**. Inside a builder read the environment through the internal `Environments` (configs plus
  explicit variables), never `DockerResourceConfiguration.Environments`, which lacks the configs.
- **Env scheme: `FLOCI_SERVICES_<SERVICE>_<SETTING>`**; `ENABLED` always emitted, other settings
  only when enabled. Top-level: `FLOCI_DEFAULT_REGION` / `FLOCI_DEFAULT_ACCOUNT_ID` /
  `FLOCI_DEFAULT_AVAILABILITY_ZONE`. Verified against the running image — match Java keys exactly.
- Endpoint is `GetEndpoint()` (`http://host:mappedPort/`); credentials are dummy `test`/`test`.
- Image: pinned `floci/floci:2.3.0` (the `FlociBuilder` default and `TestImages.Floci`; keep the
  two in sync, and in sync with `.github/docker-images.txt` for the CI cache), port 4566, native arm64.

## Gotchas

- **Ryuk on Colima**: handled by environment, not code (see "Running on Colima"). Set
  `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock` and Ryuk works. Do not add any
  Colima-specific code (e.g. a ModuleInitializer that disables Ryuk) — keep the module portable.
- **`IsExternalInit`**: required for `init` setters on netstandard2.0. We ship our own polyfill
  (`IsExternalInit.cs` in both `src` projects) so the modreq doesn't bind to a transitive
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
- **Container-based services** (RDS, Lambda, ElastiCache, ECS, EC2, ECR — all done): Floci spawns
  sibling containers via the Docker daemon. A config opts in by overriding `RequiresDockerAccess`
  **and** being listed in `AwsCloud.Descriptor.SocketServices` (`CoreTest` keeps the two in step;
  `Build()` mounts `/var/run/docker.sock` from the final env, so it is on by default) and `FixedHostPorts` (publishes ports **1:1**, since Floci
  returns `endpoint=localhost:<port>` literally). `WithServiceConfig` publishes the ports; `Build()` mounts the socket.
  ECS and EC2 also support a `Mock` mode (`RequiresDockerAccess => !Mock`) that returns RUNNING
  without spawning containers — used by their tests for determinism. Hard-won gotchas:
  - **macOS port 7000 collision**: Control Center / AirPlay Receiver listens on `*:7000`, so the
    default RDS `ProxyBasePort = 7000` is intercepted by the OS on a Mac. Tests use `7010`.
  - **Connect via `127.0.0.1`, not `localhost`** — avoids the client resolving to IPv6 (`::1`).
  - **`SSL Mode=Disable`** — Floci's RdsAuthProxy doesn't do SSL negotiation.
  - **Siblings leak**: Floci-spawned containers are Floci-managed (not Ryuk-tracked). DB/cache/
    Lambda/function siblings are named after the resource, so delete the resource in teardown
    (`DeleteDBInstance`/`DeleteReplicationGroup`/`DeleteFunction`) so Floci removes them.
  - **ECR registry persists**: ECR spawns one shared `floci-ecr-registry` (registry:2) container
    that isn't tied to a repository, and Floci's `KEEP_RUNNING_ON_SHUTDOWN=false` graceful stop
    doesn't fire under Testcontainers' abrupt kill — so it lingers after tests. It's *reused*
    (fixed name) on subsequent runs, so it doesn't accumulate or collide; just be aware it stays up.
  - **EC2 Auto Scaling is its own service**: Floci namespaces it separately
    (`FLOCI_SERVICES_AUTOSCALING_ENABLED`, not under EC2), enables it by default, and its control
    plane works independently of EC2. It has its own `AutoScalingConfig` / `WithAutoScaling` — the
    sole owner of the key. (`Ec2Config` deliberately does **not** emit it, to avoid a double-write;
    EC2 users still get Auto Scaling via Floci's default-on.) This diverges from upstream Java,
    which has no `AutoScalingConfig` and emits the key from its EC2 config.
- **Test parallelization is disabled** (`AssemblyInfo.cs`, `DisableTestParallelization = true`):
  container-backed tests starting many Floci containers at once flake under Docker-daemon load.

## Git

Commit at green checkpoints. **No `Co-Authored-By` trailers.**
