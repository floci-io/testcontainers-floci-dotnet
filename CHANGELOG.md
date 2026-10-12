# Changelog

## [1.0.1](https://github.com/floci-io/testcontainers-floci-dotnet/compare/v1.0.0...v1.0.1) (2026-10-11)


### Bug Fixes

* **deps:** Bump Testcontainers in /src/Floci.Testcontainers.Aws ([#130](https://github.com/floci-io/testcontainers-floci-dotnet/issues/130)) ([0f17b18](https://github.com/floci-io/testcontainers-floci-dotnet/commit/0f17b189507d8d9450d227f2efb51fd6fa7d9466))
* include the README in the NuGet packages ([#132](https://github.com/floci-io/testcontainers-floci-dotnet/issues/132)) ([9c09498](https://github.com/floci-io/testcontainers-floci-dotnet/commit/9c094983fa39060184e33aeb6b74ff97b65bb684))

## [1.0.0](https://github.com/floci-io/testcontainers-floci-dotnet/compare/v0.16.5...v1.0.0) (2026-10-11)


### ⚠ BREAKING CHANGES

* the package id is Floci.Testcontainers.Aws (it pulls in Floci.Testcontainers.Core) and the namespace is Floci.Testcontainers.Aws instead of Testcontainers.Floci. The README has a migration section.

### Features

* split into Floci.Testcontainers.Core and Floci.Testcontainers.Aws ([#106](https://github.com/floci-io/testcontainers-floci-dotnet/issues/106)) ([a6ddb56](https://github.com/floci-io/testcontainers-floci-dotnet/commit/a6ddb56ce55e098d38bad07ffa0c5b45f18ddc8b))


### Bug Fixes

* remove Floci's sibling containers, advertise a reachable RDS endpoint, pin floci/floci 2.2.0 ([#109](https://github.com/floci-io/testcontainers-floci-dotnet/issues/109)) ([01cf34c](https://github.com/floci-io/testcontainers-floci-dotnet/commit/01cf34cbcc01b5d7486500c4089ff633417897e4))
