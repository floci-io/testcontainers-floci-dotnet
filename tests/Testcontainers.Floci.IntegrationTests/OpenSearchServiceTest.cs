using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.OpenSearchService;
using Amazon.OpenSearchService.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class OpenSearchServiceTest : IAsyncLifetime
{
    private const string DomainName = "test-domain";
    private const int ProxyBasePort = 9400;
    private const string IndexName = "test-index";

    // Real mode: Floci spawns an actual OpenSearch container per domain and fronts it on the
    // proxy port range (published 1:1 to the host). Unlike the upstream Java test — which
    // @Disables its data-plane assertions — we drive a full round-trip against the live node.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithOpenSearch(new OpenSearchConfig { ProxyBasePort = ProxyBasePort })
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the domain so Floci tears down the sibling OpenSearch container it spawned.
        // Those siblings are Floci-managed (not Ryuk-tracked) and named after the domain, so a
        // leak would collide on the next run.
        try
        {
            using var os = CreateClient();
            await os.DeleteDomainAsync(new DeleteDomainRequest { DomainName = DomainName });
        }
        catch (AmazonOpenSearchServiceException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonOpenSearchServiceClient CreateClient()
    {
        return new AmazonOpenSearchServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonOpenSearchServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                // CreateDomain blocks while Floci pulls the OpenSearch image (~1GB) on first use,
                // which can exceed the SDK's default 100s timeout on a cold CI runner.
                Timeout = System.TimeSpan.FromMinutes(5),
            });
    }

    [Fact]
    public async Task CreatesDomainAndIndexesAndSearchesDocument()
    {
        using var os = CreateClient();

        // Control plane: create the domain and confirm it via list + describe.
        await os.CreateDomainAsync(new CreateDomainRequest { DomainName = DomainName });

        var names = await os.ListDomainNamesAsync(new ListDomainNamesRequest());
        Assert.Contains(names.DomainNames, d => d.DomainName == DomainName);

        var described = await os.DescribeDomainAsync(new DescribeDomainRequest { DomainName = DomainName });
        Assert.Equal(DomainName, described.DomainStatus.DomainName);
        Assert.False(string.IsNullOrEmpty(described.DomainStatus.ARN));

        // Data plane: Floci proxies the spawned node on the base port (plain HTTP, security off).
        // Connect via 127.0.0.1 to avoid localhost resolving to IPv6, which Testcontainers does
        // not publish. The node needs time to boot, so retry index + search until it answers.
        using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{ProxyBasePort}") };

        const string title = "Floci OpenSearch Test";
        var doc = JsonSerializer.Serialize(new { title, content = "integration testing with testcontainers" });

        var foundTitle = await IndexAndSearchWithRetryAsync(http, doc);

        Assert.Equal(title, foundTitle);
    }

    private static async Task<string> IndexAndSearchWithRetryAsync(HttpClient http, string doc)
    {
        Exception? lastError = null;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                // refresh=true makes the document immediately searchable.
                using var indexBody = new StringContent(doc, Encoding.UTF8, "application/json");
                var indexResp = await http.PutAsync($"/{IndexName}/_doc/1?refresh=true", indexBody);
                if (!indexResp.IsSuccessStatusCode)
                {
                    lastError = new Exception($"index returned {(int)indexResp.StatusCode}");
                    await Task.Delay(2000);
                    continue;
                }

                const string query = "{\"query\":{\"match\":{\"content\":\"testcontainers\"}}}";
                using var searchBody = new StringContent(query, Encoding.UTF8, "application/json");
                var searchResp = await http.PostAsync($"/{IndexName}/_search", searchBody);
                searchResp.EnsureSuccessStatusCode();

                var json = await searchResp.Content.ReadAsStringAsync();
                using var parsed = JsonDocument.Parse(json);
                var hits = parsed.RootElement.GetProperty("hits").GetProperty("hits");
                foreach (var hit in hits.EnumerateArray())
                {
                    return hit.GetProperty("_source").GetProperty("title").GetString()!;
                }

                // Indexed but not yet searchable — keep polling.
                lastError = new Exception("search returned no hits yet");
                await Task.Delay(2000);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
                await Task.Delay(2000);
            }
        }

        throw new Xunit.Sdk.XunitException(
            $"OpenSearch node did not become usable within the timeout. Last error: {lastError?.Message}");
    }
}
