# Testcontainers.Floci (.NET)

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for
[Floci](https://github.com/floci-io/floci) — a free, open-source local AWS emulator.

It spins up the `floci/floci` container for your integration tests and gives you an
endpoint to point the AWS SDK for .NET at. No account, no token.

> **Status:** early scaffold. The core container lifecycle works; per-service typed
> configuration is being added incrementally. Environment-variable keys marked
> `TODO(validate-against-floci)` still need confirming against the running image.

## Usage

```csharp
await using var floci = new FlociBuilder().Build();
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
dotnet test   # requires a running Docker daemon (Colima works)
```

## Relationship to the upstream Floci project

This module mirrors the API of the official
[`testcontainers-floci`](https://github.com/floci-io/testcontainers-floci) (Java) and
[`testcontainers-floci-python`](https://github.com/floci-io/testcontainers-floci-python)
modules. It is intended to be contributed to the `floci-io` organisation.

## License

MIT — see [LICENSE](LICENSE).
