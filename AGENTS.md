# AGENTS.md

Contributor and coding-agent guidance for this repo lives in **[CLAUDE.md](CLAUDE.md)** — the
single source of truth (project layout, the per-service template, env-var conventions, build/test
commands, and the container-based-service gotchas). Read it before making changes. This file
exists so tooling that looks for `AGENTS.md` is pointed at the same guide.

## Essentials

- **Build / test:** `dotnet build` and `dotnet test` — `dotnet test` needs a running Docker
  daemon. On Colima also set `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock`.
- **Commits:** use [Conventional Commits](https://www.conventionalcommits.org/) — they drive the
  auto-versioned release on push to `main` (`feat:` → minor, `fix:` → patch, `!` → major).
- **Adding a service:** follow the per-service template in CLAUDE.md (a `XxxConfig` record + a
  `WithXxx` builder method + a config unit test + a live integration test; match the env-var keys
  to the upstream Floci Java module exactly).

See **[CLAUDE.md](CLAUDE.md)** for the full details and the hard-won gotchas.
