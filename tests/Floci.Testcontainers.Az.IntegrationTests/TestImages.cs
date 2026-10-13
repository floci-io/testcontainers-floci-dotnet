namespace Floci.Testcontainers.Az.Tests;

/// <summary>
/// Shared image references for the integration tests. Centralised so the pinned Floci Azure tag
/// is defined in one place; keep it in sync with <c>.github/docker-images.txt</c>.
/// </summary>
internal static class TestImages
{
    public const string FlociAz = "floci/floci-az:0.14.0";
}
