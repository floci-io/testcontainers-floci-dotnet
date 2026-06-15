using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Batch;
using Amazon.Batch.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BatchServiceTest : IAsyncLifetime
{
    private const string JobDefinitionName = "test-job-def";
    private const string ComputeEnvironmentName = "test-ce";
    private const string JobQueueName = "test-queue";
    private const string JobName = "test-job";

    // Immediate runner mode: jobs complete in-process without spawning containers — deterministic
    // and no Docker socket required.
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithBatch(new BatchConfig())
        .Build();

    private string? _jobId;

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        // Best-effort teardown: Batch state is in-memory (immediate mode), but clean up for tidy
        // re-runs.
        try
        {
            using var batch = CreateClient();

            await batch.UpdateJobQueueAsync(new UpdateJobQueueRequest
            {
                JobQueue = JobQueueName,
                State = JQState.DISABLED,
            });

            await batch.DeleteJobQueueAsync(new DeleteJobQueueRequest
            {
                JobQueue = JobQueueName,
            });

            await batch.UpdateComputeEnvironmentAsync(new UpdateComputeEnvironmentRequest
            {
                ComputeEnvironment = ComputeEnvironmentName,
                State = CEState.DISABLED,
            });

            await batch.DeleteComputeEnvironmentAsync(new DeleteComputeEnvironmentRequest
            {
                ComputeEnvironment = ComputeEnvironmentName,
            });

            await batch.DeregisterJobDefinitionAsync(new DeregisterJobDefinitionRequest
            {
                JobDefinition = JobDefinitionName,
            });
        }
        catch (AmazonBatchException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonBatchClient CreateClient()
    {
        return new AmazonBatchClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonBatchConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task RegistersJobDefinitionCreatesQueueSubmitsJobAndSucceeds()
    {
        using var batch = CreateClient();

        // Register a job definition
        var registerResponse = await batch.RegisterJobDefinitionAsync(new RegisterJobDefinitionRequest
        {
            JobDefinitionName = JobDefinitionName,
            Type = JobDefinitionType.Container,
            ContainerProperties = new ContainerProperties
            {
                Image = "public.ecr.aws/docker/library/alpine:3.20",
                ResourceRequirements = new List<ResourceRequirement>
                {
                    new ResourceRequirement { Type = ResourceType.VCPU, Value = "1" },
                    new ResourceRequirement { Type = ResourceType.MEMORY, Value = "512" },
                },
            },
        });

        Assert.NotEmpty(registerResponse.JobDefinitionArn);

        // Create a compute environment (UNMANAGED — Floci manages execution, no real infra needed)
        var ceResponse = await batch.CreateComputeEnvironmentAsync(new CreateComputeEnvironmentRequest
        {
            ComputeEnvironmentName = ComputeEnvironmentName,
            Type = CEType.UNMANAGED,
            State = CEState.ENABLED,
        });

        Assert.NotEmpty(ceResponse.ComputeEnvironmentArn);

        // Create a job queue linked to the compute environment
        var queueResponse = await batch.CreateJobQueueAsync(new CreateJobQueueRequest
        {
            JobQueueName = JobQueueName,
            State = JQState.ENABLED,
            Priority = 1,
            ComputeEnvironmentOrder = new List<ComputeEnvironmentOrder>
            {
                new ComputeEnvironmentOrder { Order = 1, ComputeEnvironment = ComputeEnvironmentName },
            },
        });

        Assert.NotEmpty(queueResponse.JobQueueArn);

        // Submit a job — in immediate mode it completes in-process without a real container
        var submitResponse = await batch.SubmitJobAsync(new SubmitJobRequest
        {
            JobName = JobName,
            JobQueue = JobQueueName,
            JobDefinition = JobDefinitionName,
        });

        Assert.NotEmpty(submitResponse.JobId);
        _jobId = submitResponse.JobId;

        // Describe the job and assert it reached SUCCEEDED (immediate mode transitions synchronously)
        var describeResponse = await batch.DescribeJobsAsync(new DescribeJobsRequest
        {
            Jobs = new List<string> { _jobId },
        });

        Assert.Single(describeResponse.Jobs);
        Assert.Equal(_jobId, describeResponse.Jobs[0].JobId);
        Assert.Equal(JobStatus.SUCCEEDED, describeResponse.Jobs[0].Status);
    }
}
