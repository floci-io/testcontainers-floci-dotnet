namespace Floci.Testcontainers.Oci.Tests;

/// <summary>
/// Shared image references for the integration tests. Centralised so the pinned Floci OCI tag
/// is defined in one place; keep it in sync with <c>.github/docker-images.txt</c>.
/// </summary>
internal static class TestImages
{
    public const string FlociOci = "floci/floci-oci:0.5.0";
}
