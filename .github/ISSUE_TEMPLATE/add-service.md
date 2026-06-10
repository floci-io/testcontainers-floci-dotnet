---
name: Add AWS service support
about: Track adding a new Floci-emulated AWS service to the library
title: "Add <Service> support"
labels: new-service
---

## Service

- **AWS service:** <name>
- **Floci token:** `FLOCI_SERVICES_<TOKEN>`
- **Type:** flat / container-based <!-- container-based = Floci spawns real sibling containers -->
- **AWS SDK package:** `AWSSDK.<X>`

## References

- Floci services list: https://floci.io/floci/services/
- Java reference: `XxxConfig.java` + `XxxConfigTest.java` + `XxxServiceTest.java`
- Python reference: the `XxxConfig` in `floci/config/services.py`

## Checklist

Follows the per-service template in `CLAUDE.md`.

- [ ] `src/Testcontainers.Floci/XxxConfig.cs` — `sealed record : FlociServiceConfig`; one `init` property per setting with the **same defaults as the Java class**; `ServiceKey => "TOKEN"`; `AddSettings` writes each setting with the **exact Java env-var key** (invariant culture for numbers, `"true"`/`"false"` for bools)
- [ ] `FlociBuilder.WithXxx(XxxConfig config) => WithServiceConfig(config);`
- [ ] `tests/Testcontainers.Floci.Tests/XxxConfigTest.cs` (unit) — defaults, default env output, custom env output, disabled-only
- [ ] `tests/Testcontainers.Floci.IntegrationTests/XxxServiceTest.cs` (integration) — `IAsyncLifetime` + a real round-trip; add `AWSSDK.Xxx` to the integration project
- [ ] Container-based only: override `RequiresDockerAccess` / `FixedHostPorts`; delete the resource in teardown so Floci removes the sibling
- [ ] `dotnet build Testcontainers.Floci.slnx -c Release -warnaserror` clean and `dotnet test` green
