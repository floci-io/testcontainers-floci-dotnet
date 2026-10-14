using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Floci.Testcontainers.Oci;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Models;
using Oci.ObjectstorageService.Requests;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

public sealed class CoreServiceTest : IClassFixture<FlociOciFixture>
{
    private readonly FlociOciContainer _floci;

    public CoreServiceTest(FlociOciFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task StartsHealthy()
    {
        using var http = new HttpClient();
        using var response = await http.GetAsync(_floci.GetEndpoint() + OciCloud.Descriptor.HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OciCloud.DefaultRegion, _floci.Region);
        Assert.Equal(OciCloud.DefaultTenancyId, _floci.TenancyId);
        Assert.Equal(OciCloud.DefaultNamespace, _floci.Namespace);
    }

    [Fact]
    public void HandsOutAConfigFileProfileAndOneKeyPerContainer()
    {
        Assert.Equal(
            "[FLOCI]\n"
            + "user=ocid1.user.oc1..flocilocaluser0000000000000000000000000000000000000000000000\n"
            + "fingerprint=aa:bb:cc:dd:ee:ff:00:11:22:33:44:55:66:77:88:99\n"
            + "tenancy=ocid1.tenancy.oc1..flocilocaltenancy0000000000000000000000000000000000000000\n"
            + "region=us-ashburn-1\n"
            + "key_file=/tmp/floci_key.pem\n",
            _floci.GetConfigFileContents("/tmp/floci_key.pem", "FLOCI"));
        Assert.StartsWith("[DEFAULT]\n", _floci.GetConfigFileContents("/tmp/floci_key.pem"));

        Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----", _floci.GetPrivateKeyPem());
        Assert.Same(_floci.GetPrivateKeyPem(), _floci.GetPrivateKeyPem());
    }

    [Fact]
    public async Task ResetWipesStateWithoutRestarting()
    {
        using var client = OciSdk.ObjectStorage(_floci);
        await client.CreateBucket(new CreateBucketRequest
        {
            NamespaceName = _floci.Namespace,
            CreateBucketDetails = new CreateBucketDetails { Name = "reset-me", CompartmentId = _floci.TenancyId },
        });
        Assert.Contains(await BucketNamesAsync(client), name => name == "reset-me");

        await _floci.ResetAsync();

        Assert.DoesNotContain(await BucketNamesAsync(client), name => name == "reset-me");
    }

    [Fact]
    public async Task HasAUniqueResourceNamespacePerStartAndReportsTheConfiguredIdentity()
    {
        await using var other = new FlociOciBuilder(TestImages.FlociOci)
            .WithRegion("eu-frankfurt-1")
            .WithNamespace("other-namespace")
            .Build();
        await other.StartAsync();

        Assert.Matches("^tc-[0-9a-f]{8}$", _floci.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", other.ResourceNamespace);
        Assert.NotEqual(_floci.ResourceNamespace, other.ResourceNamespace);
        Assert.NotEqual(_floci.GetPrivateKeyPem(), other.GetPrivateKeyPem());
        Assert.Equal("my-key", new FlociOciBuilder(TestImages.FlociOci).WithPrivateKeyPem("my-key").Build().GetPrivateKeyPem());

        Assert.Equal("eu-frankfurt-1", other.Region);
        Assert.Contains("region=eu-frankfurt-1\n", other.GetConfigFileContents("/tmp/key.pem"));
        Assert.Equal("other-namespace", other.Namespace);

        // The emulator agrees on the namespace, asked through a client built from that profile.
        using var client = OciSdk.ObjectStorage(other);
        Assert.Equal("other-namespace", (await client.GetNamespace(new GetNamespaceRequest())).Value);
    }

    private async Task<string[]> BucketNamesAsync(ObjectStorageClient client)
    {
        var buckets = await client.ListBuckets(new ListBucketsRequest
        {
            NamespaceName = _floci.Namespace,
            CompartmentId = _floci.TenancyId,
        });
        return buckets.Items.Select(bucket => bucket.Name).ToArray();
    }
}
