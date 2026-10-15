namespace Floci.Testcontainers.Gcp.Tests;

/// <summary>
/// Shared image references for the integration tests. Centralised so the pinned Floci GCP tag
/// is defined in one place; keep it in sync with <c>.github/docker-images.txt</c>.
/// </summary>
internal static class TestImages
{
    public const string FlociGcp = "floci/floci-gcp:0.11.0";
}
