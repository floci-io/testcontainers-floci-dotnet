using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Floci.Testcontainers.Oci;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Models;
using Oci.ObjectstorageService.Requests;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

public sealed class ObjectStorageServiceTest : IClassFixture<FlociOciFixture>
{
    private readonly FlociOciContainer _floci;

    public ObjectStorageServiceTest(FlociOciFixture fixture)
    {
        _floci = fixture.Floci;
    }

    [Fact]
    public async Task UploadsAndDownloadsAnObject()
    {
        using var client = OciSdk.ObjectStorage(_floci);

        var ns = (await client.GetNamespace(new GetNamespaceRequest())).Value;
        Assert.Equal(_floci.Namespace, ns);

        await client.CreateBucket(new CreateBucketRequest
        {
            NamespaceName = ns,
            CreateBucketDetails = new CreateBucketDetails { Name = "orders", CompartmentId = _floci.TenancyId },
        });

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello from floci"));
        await client.PutObject(new PutObjectRequest
        {
            NamespaceName = ns,
            BucketName = "orders",
            ObjectName = "order-1.txt",
            PutObjectBody = content,
        });

        var downloaded = await client.GetObject(new GetObjectRequest
        {
            NamespaceName = ns,
            BucketName = "orders",
            ObjectName = "order-1.txt",
        });
        using var reader = new StreamReader(downloaded.InputStream, Encoding.UTF8);
        Assert.Equal("hello from floci", await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task TheEmulatorReadsTheTenancyFromTheSignedRequest()
    {
        // The emulator keeps resources per tenancy and takes the tenancy from the signature the SDK
        // builds out of the config-file profile, so this only passes if that profile reaches it intact.
        using var mine = OciSdk.ObjectStorage(_floci, profile: "FLOCI");
        await mine.CreateBucket(new CreateBucketRequest
        {
            NamespaceName = _floci.Namespace,
            CreateBucketDetails = new CreateBucketDetails { Name = "tenancy-check", CompartmentId = _floci.TenancyId },
        });
        Assert.Contains(await BucketNamesAsync(mine), name => name == "tenancy-check");

        const string otherTenancy = "ocid1.tenancy.oc1..anothertenancy000000000000000000000000000000000000000000";
        using var theirs = OciSdk.ObjectStorage(_floci, edit: config => config.Replace(_floci.TenancyId, otherTenancy));
        Assert.DoesNotContain(await BucketNamesAsync(theirs), name => name == "tenancy-check");
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
