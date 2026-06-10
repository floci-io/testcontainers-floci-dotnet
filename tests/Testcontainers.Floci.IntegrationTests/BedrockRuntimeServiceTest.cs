using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BedrockRuntimeServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonBedrockRuntimeClient CreateClient()
    {
        return new AmazonBedrockRuntimeClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonBedrockRuntimeConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task InvokesModel()
    {
        using var bedrock = CreateClient();
        const string payload = "{\"inputText\":\"Hello\",\"textGenerationConfig\":{\"maxTokenCount\":10}}";

        var response = await bedrock.InvokeModelAsync(new InvokeModelRequest
        {
            ModelId = "amazon.titan-text-lite-v1",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(payload)),
            ContentType = "application/json",
            Accept = "application/json",
        });

        Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
        Assert.NotNull(response.Body);
    }
}
