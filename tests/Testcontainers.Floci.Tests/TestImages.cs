namespace Testcontainers.Floci.Tests;

/// <summary>
/// Shared image references for the integration tests. Centralised so the pinned Floci tag is
/// defined in one place and tests use the image-parameter constructor (not the obsolete
/// parameterless one).
/// </summary>
internal static class TestImages
{
    public const string Floci = "floci/floci:1.5.22";
}
