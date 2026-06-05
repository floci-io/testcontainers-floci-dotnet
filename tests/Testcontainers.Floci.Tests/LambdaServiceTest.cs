using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class LambdaServiceTest : IAsyncLifetime
{
    private const string FunctionName = "test-function";

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithLambda(new LambdaConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Remove the function so Floci tears down its runtime container.
        try
        {
            using var lambda = CreateClient();
            await lambda.DeleteFunctionAsync(new DeleteFunctionRequest { FunctionName = FunctionName });
        }
        catch (AmazonLambdaException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonLambdaClient CreateClient()
    {
        return new AmazonLambdaClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonLambdaConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
                Timeout = TimeSpan.FromMinutes(2), // first invoke is a cold start
            });
    }

    [Fact]
    public async Task CreatesFunctionAndInvokesRealCode()
    {
        using var lambda = CreateClient();

        await lambda.CreateFunctionAsync(new CreateFunctionRequest
        {
            FunctionName = FunctionName,
            Runtime = Amazon.Lambda.Runtime.Python312,
            Handler = "lambda_function.handler",
            Role = "arn:aws:iam::000000000000:role/lambda-role",
            Code = new FunctionCode { ZipFile = BuildPythonHandlerZip() },
        });

        var response = await lambda.InvokeAsync(new InvokeRequest
        {
            FunctionName = FunctionName,
            Payload = "{\"hello\":\"world\"}",
            InvocationType = InvocationType.RequestResponse,
        });

        using var reader = new StreamReader(response.Payload);
        var payload = await reader.ReadToEndAsync();

        Assert.Equal(200, response.StatusCode);
        Assert.True(string.IsNullOrEmpty(response.FunctionError), $"Function error: {response.FunctionError}; payload: {payload}");
        // Real execution: the handler echoes the input and returns its message.
        Assert.Contains("hello from floci lambda", payload);
        Assert.Contains("world", payload);
    }

    private static MemoryStream BuildPythonHandlerZip()
    {
        const string source =
            "def handler(event, context):\n" +
            "    return {\"echo\": event, \"message\": \"hello from floci lambda\"}\n";

        var zip = new MemoryStream();
        using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("lambda_function.py");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(source);
        }

        zip.Position = 0;
        return zip;
    }
}
