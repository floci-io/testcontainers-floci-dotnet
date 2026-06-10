using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CodeBuild;
using Amazon.CodeBuild.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class CodeBuildServiceTest : IAsyncLifetime
{
    private const string ProjectName = "floci-test";

    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithCodeBuild(new CodeBuildConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public async Task DisposeAsync()
    {
        try
        {
            using var cb = CreateClient();
            await cb.DeleteProjectAsync(new DeleteProjectRequest { Name = ProjectName });
        }
        catch (AmazonCodeBuildException)
        {
            // The container is being disposed anyway; nothing actionable here.
        }

        await _floci.DisposeAsync();
    }

    private AmazonCodeBuildClient CreateClient()
    {
        return new AmazonCodeBuildClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonCodeBuildConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task RunsABuildToCompletion()
    {
        using var cb = CreateClient();

        // Unlike the upstream Java test (which only lists projects), we run a real build: Floci
        // spawns a build container, executes the buildspec, and reports the terminal status. A
        // tiny alpine image keeps the build container pull cheap. (Verified separately that a
        // failing buildspec yields FAILED, so SUCCEEDED genuinely reflects buildspec execution.)
        await cb.CreateProjectAsync(new CreateProjectRequest
        {
            Name = ProjectName,
            Source = new ProjectSource
            {
                Type = SourceType.NO_SOURCE,
                Buildspec = "version: 0.2\nphases:\n  build:\n    commands:\n      - echo hello-from-floci",
            },
            Artifacts = new ProjectArtifacts { Type = ArtifactsType.NO_ARTIFACTS },
            Environment = new ProjectEnvironment
            {
                Type = EnvironmentType.LINUX_CONTAINER,
                Image = "public.ecr.aws/docker/library/alpine:3.20",
                ComputeType = ComputeType.BUILD_GENERAL1_SMALL,
            },
            ServiceRole = "arn:aws:iam::000000000000:role/codebuild-role",
        });

        var started = await cb.StartBuildAsync(new StartBuildRequest { ProjectName = ProjectName });
        var buildId = started.Build.Id;

        var status = await WaitForTerminalStatusAsync(cb, buildId);

        Assert.Equal(StatusType.SUCCEEDED, status);
    }

    private static async Task<StatusType> WaitForTerminalStatusAsync(AmazonCodeBuildClient cb, string buildId)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var builds = await cb.BatchGetBuildsAsync(new BatchGetBuildsRequest
            {
                Ids = new List<string> { buildId },
            });

            var status = builds.Builds[0].BuildStatus;
            if (status != StatusType.IN_PROGRESS)
            {
                return status;
            }

            await Task.Delay(2000);
        }

        throw new Xunit.Sdk.XunitException("CodeBuild build did not reach a terminal status within the timeout.");
    }
}
