# Contributing to Testcontainers.Floci (.NET)

Thanks for your interest in contributing! This explains how to build and test the module, the
branching model, and the conventions to follow.

Please also read the [Code of Conduct](CODE_OF_CONDUCT.md).

**Join us on [Slack](https://join.slack.com/t/floci/shared_invite/zt-3tjn02s3q-A00kEjJ1cZxsg_imTfy6Cw)**: it is the fastest way to reach maintainers.

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

## Developer Certificate of Origin (DCO) sign-off

Every commit must be **signed off**, certifying the
[Developer Certificate of Origin](https://developercertificate.org/), a lightweight statement
that you wrote the contribution or otherwise have the right to submit it under the project's
license. This keeps Floci's licensing clean and unambiguous, and it is **required for a pull
request to be merged**.

Sign off by adding the `-s` flag when you commit:

```bash
git commit -s -m "feat(s3): add multipart upload copy-part support"
```

This appends a `Signed-off-by: Your Name <your@email>` trailer using your configured git
identity. If you forget, you can amend the most recent commit with `git commit --amend -s`, or
sign off a range during an interactive rebase.

### Why the DCO and not a CLA

Floci is built by the community, for the community, and the DCO is how it stays that way. There
is no agreement to sign and no rights to hand over. You certify that the work is yours to give,
you keep the copyright in it, and it reaches everyone else on the same MIT terms it arrived
under.

A CLA would ask every contributor to grant something extra to whoever holds the project. Floci
does not ask for that. The Lead Maintainer signs off the same way a first-time contributor
does, and holds no rights over your work that you do not hold over theirs. Code released under
MIT stays under MIT: free to use, fork, and build on, for anyone, permanently.

Changes to this policy are reserved to the Lead Maintainer under
[GOVERNANCE.md](https://github.com/floci-io/.github/blob/main/GOVERNANCE.md).

## Commit messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/), checked in CI (`commit-lint`).
Releases follow the same release-please flow as the Java module (`.github/workflows/release-please.yml`):
every push to `main` updates one **release PR** with the next version and the `CHANGELOG.md` entry, and
nothing is published until a maintainer merges it. Merging tags `vX.Y.Z`, creates the GitHub release,
packs the packages and pushes them to nuget.org, and runs the security scans (CodeQL, Trivy) against the tag.
Publishing uses nuget.org trusted publishing: a policy owned by the `floci` organization trusts this
workflow for the `Floci.Testcontainers.*` packages, so no API key is stored. The repository variable
`NUGET_USER` names the nuget.org account that created the policy.

| Prefix | Version bump | Example |
|---|---|---|
| `fix:` / `perf:` | Patch | `fix: handle null region gracefully` |
| `feat:` | Minor | `feat: add WithOpenSearch configuration` |
| `feat!:` or `BREAKING CHANGE:` | Major | `feat!: rename GetEndpoint to GetServiceUrl` |
| `chore:` / `ci:` / `docs:` / `test:` / `refactor:` | none | `chore: bump AWSSDK.S3` |

The version lives in `Directory.Build.props` (and `version.txt`), bumped only by release-please. To recover
a release (for example a failed push), run **Release Please** manually with the `tag` input.

**No AI attribution.** Do not add "Generated by", "Co-Authored-By: …-bot", or similar trailers to commit messages. Attribution should be limited to human contributors.

## Pull Request Limits and Review Bandwidth

To make sure every contribution gets a thorough, high-quality review in a reasonable time, we ask contributors to keep **no more than 4 open pull requests**, drafts included, at any time in this repository, and **no more than 2 of them ready for review**.

- **Why this policy exists:** maintainer review time is limited. Capping concurrent open PRs prevents review backlogs, reduces context switching, and keeps PR cycle times short for everyone.
- **Dependent work:** if your work depends on a PR that has not been merged yet, build on that branch or note the dependency in the discussion instead of opening separate, uncoordinated PRs.
- **Draft PRs:** drafts do not count against the limit of 2 ready pull requests, but they do count toward the total of 4. You can have, for example, 2 ready and 2 drafts, or 1 ready and 3 drafts. Use drafts for work in progress, not as a queue of finished changes waiting for a review slot, and mark a draft as ready for review only when you have review capacity available.
- **How it is applied:** a bot turns a pull request back into a draft if it would be your 3rd ready for review, and closes a pull request opened while you already have 4 open, drafts included. Your branch and commits are always kept: mark the draft ready again once one of your ready pull requests is merged, closed or turned into a draft, and reopen a closed pull request once you have fewer than 4 open. Maintainers and dependency bots are not counted.

Once your current pull requests are reviewed, merged, or closed, you are welcome to open new ones!

## Releases

Releases are automated. A push to `main` computes the next version from the commit history, tags it, creates a
GitHub Release, and publishes the package. Contributors don't manage versions or tags.

## Reporting Security Issues

Please do **not** open public issues for security vulnerabilities. See [SECURITY.md](SECURITY.md) for how to report them privately.
