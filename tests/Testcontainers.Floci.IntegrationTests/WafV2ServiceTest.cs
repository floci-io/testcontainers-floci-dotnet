using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.WAFV2;
using Amazon.WAFV2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class WafV2ServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithWafV2(new WafV2Config())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonWAFV2Client CreateClient()
    {
        return new AmazonWAFV2Client(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonWAFV2Config
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsWebAcl()
    {
        using var waf = CreateClient();
        const string name = "test-web-acl";
        string? id = null;
        string? lockToken = null;

        try
        {
            var createResponse = await waf.CreateWebACLAsync(new CreateWebACLRequest
            {
                Name = name,
                Scope = Scope.REGIONAL,
                DefaultAction = new DefaultAction { Allow = new AllowAction() },
                VisibilityConfig = new VisibilityConfig
                {
                    SampledRequestsEnabled = false,
                    CloudWatchMetricsEnabled = false,
                    MetricName = "test",
                },
            });

            Assert.Equal(HttpStatusCode.OK, createResponse.HttpStatusCode);
            id = createResponse.Summary.Id;
            lockToken = createResponse.Summary.LockToken;

            var listResponse = await waf.ListWebACLsAsync(new ListWebACLsRequest
            {
                Scope = Scope.REGIONAL,
            });

            Assert.Equal(HttpStatusCode.OK, listResponse.HttpStatusCode);
            Assert.Contains(listResponse.WebACLs, acl => acl.Name == name && acl.Id == id);
        }
        finally
        {
            if (id != null && lockToken != null)
            {
                try
                {
                    await waf.DeleteWebACLAsync(new DeleteWebACLRequest
                    {
                        Name = name,
                        Id = id,
                        LockToken = lockToken,
                        Scope = Scope.REGIONAL,
                    });
                }
                catch (AmazonWAFV2Exception)
                {
                    // Best-effort cleanup.
                }
            }
        }
    }
}
