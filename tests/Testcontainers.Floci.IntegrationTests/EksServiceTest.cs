using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.EKS;
using Amazon.EKS.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class EksServiceTest : IAsyncLifetime
{
    private const string ClusterName = "eks-test";
    private const string Namespace = "floci-test";

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithEks(new EksConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Delete the cluster so Floci tears down the sibling k3s container it spawned.
        try
        {
            using var eks = CreateClient();
            await eks.DeleteClusterAsync(new DeleteClusterRequest { Name = ClusterName });
        }
        catch (AmazonEKSException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonEKSClient CreateClient()
    {
        return new AmazonEKSClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonEKSConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                // CreateCluster can block while Floci pulls the k3s image on first use.
                Timeout = TimeSpan.FromMinutes(5),
            });
    }

    [Fact]
    public async Task CreatesClusterAndDrivesKubernetesApi()
    {
        using var eks = CreateClient();

        // Control plane: create the cluster and wait for it to become ACTIVE.
        await eks.CreateClusterAsync(new CreateClusterRequest
        {
            Name = ClusterName,
            RoleArn = "arn:aws:iam::000000000000:role/eks-role",
            ResourcesVpcConfig = new VpcConfigRequest(),
        });

        var cluster = await WaitForActiveClusterAsync(eks);
        Assert.False(string.IsNullOrEmpty(cluster.Endpoint));

        // Floci returns the API server as https://localhost:<port>; connect via 127.0.0.1 to avoid
        // resolving to IPv6 (::1), which Testcontainers does not publish.
        var apiPort = new Uri(cluster.Endpoint).Port;
        var k8sBase = $"https://127.0.0.1:{apiPort}";
        var stsHost = new Uri(_floci.GetEndpoint()).Authority;

        using var k8s = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        });

        // Data plane: authenticate with an EKS bearer token (the `aws eks get-token` scheme) and
        // drive the real Kubernetes API — create a namespace + ConfigMap and read it back. This
        // goes beyond control-plane checks and exercises the live k3s API server end to end.
        await WaitForKubernetesReadyAsync(k8s, k8sBase, stsHost);

        await PostJsonAsync(k8s, $"{k8sBase}/api/v1/namespaces", stsHost,
            "{\"metadata\":{\"name\":\"" + Namespace + "\"}}");

        await PostJsonAsync(k8s, $"{k8sBase}/api/v1/namespaces/{Namespace}/configmaps", stsHost,
            "{\"metadata\":{\"name\":\"test-config\"},\"data\":{\"greeting\":\"hello-from-floci\"}}");

        using var read = await SendAsync(k8s, HttpMethod.Get,
            $"{k8sBase}/api/v1/namespaces/{Namespace}/configmaps/test-config", stsHost, body: null);
        read.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        var greeting = doc.RootElement.GetProperty("data").GetProperty("greeting").GetString();
        Assert.Equal("hello-from-floci", greeting);
    }

    private async Task<Cluster> WaitForActiveClusterAsync(AmazonEKSClient eks)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var cluster = (await eks.DescribeClusterAsync(new DescribeClusterRequest { Name = ClusterName })).Cluster;
            if (cluster.Status == ClusterStatus.ACTIVE)
            {
                return cluster;
            }

            Assert.NotEqual(ClusterStatus.FAILED, cluster.Status);
            await Task.Delay(2000);
        }

        throw new Xunit.Sdk.XunitException("EKS cluster did not become ACTIVE within the timeout.");
    }

    private async Task WaitForKubernetesReadyAsync(HttpClient k8s, string k8sBase, string stsHost)
    {
        Exception? lastError = null;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                using var resp = await SendAsync(k8s, HttpMethod.Get, $"{k8sBase}/api/v1/namespaces", stsHost, body: null);
                if (resp.IsSuccessStatusCode)
                {
                    return;
                }

                lastError = new Exception($"namespaces list returned {(int)resp.StatusCode}");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
            }

            await Task.Delay(2000);
        }

        throw new Xunit.Sdk.XunitException(
            $"Kubernetes API did not become usable within the timeout. Last error: {lastError?.Message}");
    }

    private async Task PostJsonAsync(HttpClient k8s, string url, string stsHost, string json)
    {
        using var resp = await SendAsync(k8s, HttpMethod.Post, url, stsHost, json);
        resp.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpClient k8s, HttpMethod method, string url, string stsHost, string? body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + GenerateEksToken(stsHost));
        if (body != null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return await k8s.SendAsync(request);
    }

    // Builds an EKS bearer token: a SigV4 query-presigned STS GetCallerIdentity URL (carrying the
    // x-k8s-aws-id header) base64url-wrapped as "k8s-aws-v1.<url>" — exactly what `aws eks
    // get-token` produces. Floci's IAM-auth webhook validates it and maps it to cluster-admin.
    private string GenerateEksToken(string stsHost)
    {
        const string service = "sts";
        var region = _floci.Region;
        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["Action"] = "GetCallerIdentity",
            ["Version"] = "2011-06-15",
            ["X-Amz-Algorithm"] = "AWS4-HMAC-SHA256",
            ["X-Amz-Credential"] = $"{_floci.AccessKey}/{dateStamp}/{region}/{service}/aws4_request",
            ["X-Amz-Date"] = amzDate,
            ["X-Amz-Expires"] = "900",
            ["X-Amz-SignedHeaders"] = "host;x-k8s-aws-id",
        };
        var canonicalQuery = string.Join("&", query.Select(kv => $"{UriEncode(kv.Key)}={UriEncode(kv.Value)}"));

        var canonicalRequest =
            $"GET\n/\n{canonicalQuery}\nhost:{stsHost}\nx-k8s-aws-id:{ClusterName}\n\nhost;x-k8s-aws-id\n{Hex(Sha256(string.Empty))}";
        var scope = $"{dateStamp}/{region}/{service}/aws4_request";
        var stringToSign = $"AWS4-HMAC-SHA256\n{amzDate}\n{scope}\n{Hex(Sha256(canonicalRequest))}";

        var signingKey = HmacSha256(
            HmacSha256(
                HmacSha256(
                    HmacSha256(Encoding.UTF8.GetBytes("AWS4" + _floci.SecretKey), dateStamp),
                    region),
                service),
            "aws4_request");
        var signature = Hex(HmacSha256(signingKey, stringToSign));

        var presignedUrl = $"http://{stsHost}/?{canonicalQuery}&X-Amz-Signature={signature}";
        return "k8s-aws-v1." + Base64UrlNoPad(Encoding.UTF8.GetBytes(presignedUrl));
    }

    private static string UriEncode(string value)
    {
        var sb = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')
                || c is '-' or '_' or '.' or '~')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return sb.ToString();
    }

    private static string Base64UrlNoPad(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

    private static byte[] Sha256(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));

    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
