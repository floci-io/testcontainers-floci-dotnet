using System.Threading.Tasks;
using Amazon.ConfigService;
using Amazon.ConfigService.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class AwsConfigServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithAwsConfig(new AwsConfigConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonConfigServiceClient CreateClient()
    {
        return new AmazonConfigServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonConfigServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task DescribeConfigurationRecorders_ReturnsSuccessfully()
    {
        using var client = CreateClient();
        var response = await client.DescribeConfigurationRecordersAsync(new DescribeConfigurationRecordersRequest());
        Assert.NotNull(response);
    }
}
