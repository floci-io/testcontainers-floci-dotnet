using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class SsmServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonSimpleSystemsManagementClient CreateClient()
    {
        return new AmazonSimpleSystemsManagementClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonSimpleSystemsManagementConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task PutsAndGetsParameter()
    {
        using var ssm = CreateClient();
        const string name = "/test/param";
        const string value = "hello";

        await ssm.PutParameterAsync(new PutParameterRequest
        {
            Name = name,
            Value = value,
            Type = ParameterType.String,
        });

        var response = await ssm.GetParameterAsync(new GetParameterRequest { Name = name });

        Assert.Equal(value, response.Parameter.Value);
    }
}
