using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CloudMapServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithCloudMap(new CloudMapConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonServiceDiscoveryClient CreateClient()
    {
        return new AmazonServiceDiscoveryClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonServiceDiscoveryConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task RegistersAndDiscoversInstance()
    {
        using var sd = CreateClient();

        const string namespaceName = "test-namespace";
        const string serviceName = "test-service";
        const string instanceId = "inst-1";

        string? namespaceId = null;
        string? serviceId = null;

        try
        {
            // Create an HTTP namespace — returns an OperationId because it's async.
            var createNsResponse = await sd.CreateHttpNamespaceAsync(new CreateHttpNamespaceRequest
            {
                Name = namespaceName,
            });

            var namespaceOperationId = createNsResponse.OperationId;

            // Poll until SUCCESS. With OperationCompletionDelaySeconds=0 this is immediate, but
            // we loop a few times with a short back-off to handle any in-flight processing.
            namespaceId = await PollOperationToSuccessAsync(sd, namespaceOperationId, "NAMESPACE");

            // Create a service within the namespace.
            var createSvcResponse = await sd.CreateServiceAsync(new CreateServiceRequest
            {
                Name = serviceName,
                NamespaceId = namespaceId,
            });

            serviceId = createSvcResponse.Service.Id;

            // Register an instance.
            var registerResponse = await sd.RegisterInstanceAsync(new RegisterInstanceRequest
            {
                ServiceId = serviceId,
                InstanceId = instanceId,
                Attributes = new Dictionary<string, string>
                {
                    ["AWS_INSTANCE_IPV4"] = "10.0.0.1",
                },
            });

            await PollOperationToSuccessAsync(sd, registerResponse.OperationId, target: null);

            // List instances via the control-plane API and assert the registered one is present.
            // (DiscoverInstancesAsync targets a separate data-plane endpoint that isn't reachable
            // from the emulator's single ServiceURL; ListInstancesAsync uses the same endpoint.)
            var listResponse = await sd.ListInstancesAsync(new ListInstancesRequest
            {
                ServiceId = serviceId,
            });

            Assert.Contains(listResponse.Instances, i => i.Id == instanceId);
        }
        finally
        {
            // Best-effort teardown — ignore failures so the test can report its own result.
            try
            {
                if (serviceId != null)
                {
                    var deregResponse = await sd.DeregisterInstanceAsync(new DeregisterInstanceRequest
                    {
                        ServiceId = serviceId,
                        InstanceId = instanceId,
                    });
                    await PollOperationToSuccessAsync(sd, deregResponse.OperationId, target: null);
                }
            }
            catch { /* best-effort */ }

            try
            {
                if (serviceId != null)
                {
                    await sd.DeleteServiceAsync(new DeleteServiceRequest { Id = serviceId });
                }
            }
            catch { /* best-effort */ }

            try
            {
                if (namespaceId != null)
                {
                    await sd.DeleteNamespaceAsync(new DeleteNamespaceRequest { Id = namespaceId });
                }
            }
            catch { /* best-effort */ }
        }
    }

    /// <summary>
    /// Polls <see cref="AmazonServiceDiscoveryClient.GetOperationAsync" /> until the operation
    /// reaches <see cref="OperationStatus.SUCCESS" /> and returns the value of
    /// <paramref name="target" /> from <c>Operation.Targets</c> (or <see langword="null" /> when
    /// no target value is needed).
    /// </summary>
    private static async Task<string?> PollOperationToSuccessAsync(
        AmazonServiceDiscoveryClient client,
        string operationId,
        string? target)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var response = await client.GetOperationAsync(new GetOperationRequest
            {
                OperationId = operationId,
            });

            if (response.Operation.Status == OperationStatus.SUCCESS)
            {
                if (target == null)
                {
                    return null;
                }

                response.Operation.Targets.TryGetValue(target, out var value);
                return value;
            }

            if (response.Operation.Status == OperationStatus.FAIL)
            {
                throw new InvalidOperationException(
                    $"Cloud Map operation {operationId} failed: {response.Operation.ErrorMessage}");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new TimeoutException($"Cloud Map operation {operationId} did not reach SUCCESS within the polling limit.");
    }
}
