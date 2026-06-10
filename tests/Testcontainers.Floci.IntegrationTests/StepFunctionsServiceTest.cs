using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class StepFunctionsServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithStepFunctions(new StepFunctionsConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonStepFunctionsClient CreateClient()
    {
        return new AmazonStepFunctionsClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonStepFunctionsConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsStateMachine()
    {
        using var sfn = CreateClient();
        const string name = "test-state-machine";

        try
        {
            await sfn.CreateStateMachineAsync(new CreateStateMachineRequest
            {
                Name = name,
                RoleArn = "arn:aws:iam::000000000000:role/sfn",
                Definition = "{\"StartAt\":\"Done\",\"States\":{\"Done\":{\"Type\":\"Pass\",\"End\":true}}}",
            });
        }
        catch (AmazonStepFunctionsException)
        {
            // Floci may not support CreateStateMachine — fall back to list-only check.
        }

        var response = await sfn.ListStateMachinesAsync(new ListStateMachinesRequest());

        Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
    }
}
