using JetBrains.Annotations;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// The Oracle Cloud Infrastructure emulator (<c>floci/floci-oci</c>) as a
/// <see cref="CloudDescriptor" />, plus its identity defaults.
/// </summary>
[PublicAPI]
public static class OciCloud
{
    /// <summary>The emulator's default region.</summary>
    public const string DefaultRegion = "us-ashburn-1";

    /// <summary>The realm of the OCIDs the emulator mints (the commercial realm).</summary>
    public const string DefaultRealm = "oc1";

    /// <summary>The OCID of the tenancy the emulator uses for requests that carry no signature.</summary>
    public const string DefaultTenancyId = "ocid1.tenancy.oc1..flocilocaltenancy0000000000000000000000000000000000000000";

    /// <summary>The Object Storage namespace of the emulated tenancy.</summary>
    public const string DefaultNamespace = "floci-local";

    /// <summary>
    /// The user OCID of the credentials the container hands out. The emulator has no user
    /// directory to check it against; it only has to be shaped like an OCID.
    /// </summary>
    public const string DefaultUserId = "ocid1.user.oc1..flocilocaluser0000000000000000000000000000000000000000000000";

    /// <summary>
    /// The API key fingerprint of the credentials the container hands out. The emulator never
    /// verifies request signatures, so it does not have to match the key.
    /// </summary>
    public const string DefaultFingerprint = "aa:bb:cc:dd:ee:ff:00:11:22:33:44:55:66:77:88:99";

    /// <summary>The env var that sets the region.</summary>
    public const string RegionEnv = "FLOCI_OCI_DEFAULT_REGION";

    /// <summary>The env var that sets the default tenancy.</summary>
    public const string TenancyIdEnv = "FLOCI_OCI_DEFAULT_TENANCY_ID";

    /// <summary>The env var that sets the Object Storage namespace.</summary>
    public const string NamespaceEnv = "FLOCI_OCI_DEFAULT_NAMESPACE";

    /// <summary>The env var that sets the emulator's log level.</summary>
    public const string LogLevelEnv = "QUARKUS_LOG_CATEGORY__IO_FLOCI_OCI__LEVEL";

    /// <summary>
    /// Gets the OCI emulator's descriptor. Its socket services are the configs that can need the
    /// Docker socket; a unit test keeps the two in step.
    /// </summary>
    public static CloudDescriptor Descriptor { get; } = new(
        Name: "oci",
        Port: 4599,
        EnvPrefix: "FLOCI_OCI_",
        HealthPath: "/_floci-oci/health",
        ResetPath: "/_floci-oci/state/reset",
        SocketServices: new[]
        {
            new SocketService("FUNCTIONS", Mockable: true),
            new SocketService("OKE", Mockable: true),
        });
}
