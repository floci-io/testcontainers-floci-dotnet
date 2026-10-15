# Changelog

## [1.1.0](https://github.com/floci-io/testcontainers-floci-dotnet/compare/v1.0.1...v1.1.0) (2026-10-15)


### Features

* **az:** add Floci.Testcontainers.Az for the Azure emulator ([#133](https://github.com/floci-io/testcontainers-floci-dotnet/issues/133)) ([7cafae9](https://github.com/floci-io/testcontainers-floci-dotnet/commit/7cafae960d64139005bb75776d3ea136c43dc08e))
* **gcp:** add Floci.Testcontainers.Gcp for the Google Cloud emulator ([#136](https://github.com/floci-io/testcontainers-floci-dotnet/issues/136)) ([440a754](https://github.com/floci-io/testcontainers-floci-dotnet/commit/440a7540f1568bebf3a8b1daba1b69b8cd95a247))
* **oci:** add Floci.Testcontainers.Oci for the Oracle Cloud Infrastructure emulator ([#137](https://github.com/floci-io/testcontainers-floci-dotnet/issues/137)) ([6032428](https://github.com/floci-io/testcontainers-floci-dotnet/commit/60324284f3de8f351539f7d43386125c164b437e))


### Bug Fixes

* default to floci/floci:2.3.0 and pin the new emulator images ([#139](https://github.com/floci-io/testcontainers-floci-dotnet/issues/139)) ([0125eeb](https://github.com/floci-io/testcontainers-floci-dotnet/commit/0125eebce4b5532049a1a6ec300f73f04f342c4b))
* drop a replaced config's settings ([#134](https://github.com/floci-io/testcontainers-floci-dotnet/issues/134)) ([a446dfd](https://github.com/floci-io/testcontainers-floci-dotnet/commit/a446dfd257289497dd3d2401b5faed2914ff4416))

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
