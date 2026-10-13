using System;
using System.Collections.Generic;
using System.Linq;
using Floci.Testcontainers.Aws;
using Xunit;

namespace Floci.Testcontainers.Aws.Tests;

public sealed class CoreTest
{
    private const string Image = "floci/floci:latest";

    /// <summary>Every config with a socket rule, enabled and disabled, mocked and real.</summary>
    public static IEnumerable<object[]> SocketConfigs() => new FlociServiceConfig[]
    {
        new BatchConfig(), new BatchConfig { RunnerMode = "docker" },
        new CodeBuildConfig(), new CodeBuildConfig { Enabled = false },
        new Ec2Config(), new Ec2Config { Mock = true },
        new EcrConfig(),
        new EcsConfig(), new EcsConfig { Mock = true },
        new EksConfig(), new EksConfig { Mock = true },
        new ElastiCacheConfig(),
        new LambdaConfig(), new LambdaConfig { Enabled = false },
        new MskConfig(), new MskConfig { Mock = true },
        new NeptuneConfig(),
        new OpenSearchConfig(), new OpenSearchConfig { Mock = true },
        new RdsConfig(),
        new SqsConfig(),
    }.Select(config => new object[] { config });

    [Theory]
    [MemberData(nameof(SocketConfigs))]
    public void DescriptorAgreesWithEachConfig(FlociServiceConfig config)
    {
        var env = new Dictionary<string, string>(config.BuildEnvironment());
        var only = AwsCloud.Descriptor with
        {
            SocketServices = AwsCloud.Descriptor.SocketServices.Where(svc => svc.Token == config.ServiceToken).ToArray(),
        };

        Assert.Equal(config.Enabled && config.RequiresDockerAccess, only.DockerSocketRequired(env));
    }

    [Fact]
    public void EveryConfigThatNeedsTheSocketIsInTheDescriptor()
    {
        var tokens = typeof(FlociBuilder).Assembly.GetTypes()
            .Where(type => typeof(FlociServiceConfig).IsAssignableFrom(type) && !type.IsAbstract)
            .Select(type => (FlociServiceConfig)Activator.CreateInstance(type)!)
            .Where(config => config.RequiresDockerAccess || config is BatchConfig)
            .Select(config => config.ServiceToken);

        foreach (var token in tokens)
        {
            Assert.Contains(AwsCloud.Descriptor.SocketServices, svc => svc.Token == token);
        }
    }

    [Fact]
    public void MountsTheSocketByDefaultSinceContainerServicesAreEnabledByDefault()
    {
        Assert.Equal(FlociHost.HostDockerSocket(), new FlociBuilder(Image).DockerSocketMount());
    }

    [Fact]
    public void SkipsTheSocketWhenNoContainerServiceIsEnabled()
    {
        Assert.Null(WithoutSocketServices().DockerSocketMount());
    }

    [Fact]
    public void ReadsTheFinalEnvironmentIncludingRawOverrides()
    {
        var builder = WithoutSocketServices().WithEnvironment("FLOCI_SERVICES_LAMBDA_ENABLED", "true");

        Assert.Equal(FlociHost.HostDockerSocket(), builder.DockerSocketMount());
    }

    [Fact]
    public void ExplicitChoicesWinBothWays()
    {
        Assert.Null(new FlociBuilder(Image).WithoutDockerSocket().DockerSocketMount());
        Assert.Equal("/run/user/1000/docker.sock", WithoutSocketServices().WithDockerSocket("/run/user/1000/docker.sock").DockerSocketMount());
    }

    [Fact]
    public void GivesEachContainerAUniqueOverridableResourceNamespace()
    {
        // Build picks the namespace, so two containers from one builder get different ones.
        var builder = new FlociBuilder(Image);
        var first = builder.WithDefaultResourceNamespace().ResourceNamespace;
        var second = builder.WithDefaultResourceNamespace().ResourceNamespace;

        Assert.Null(builder.ResourceNamespace);
        Assert.Matches("^tc-[0-9a-f]{8}$", first);
        Assert.NotEqual(first, second);

        // A namespace the caller sets, either way, is kept.
        Assert.Equal("ci-42", builder.WithResourceNamespace("ci-42").WithDefaultResourceNamespace().ResourceNamespace);
        Assert.Equal("ci-43", builder.WithEnvironment(AwsCloud.Descriptor.ResourceNamespaceEnv, "ci-43").WithDefaultResourceNamespace().ResourceNamespace);
    }

    private static FlociBuilder WithoutSocketServices() => new FlociBuilder(Image)
        .WithBatch(new BatchConfig())
        .WithCodeBuild(new CodeBuildConfig { Enabled = false })
        .WithEc2(new Ec2Config { Enabled = false })
        .WithEcr(new EcrConfig { Enabled = false })
        .WithEcs(new EcsConfig { Enabled = false })
        .WithEks(new EksConfig { Enabled = false })
        .WithElastiCache(new ElastiCacheConfig { Enabled = false })
        .WithLambda(new LambdaConfig { Enabled = false })
        .WithMsk(new MskConfig { Enabled = false })
        .WithNeptune(new NeptuneConfig { Enabled = false })
        .WithOpenSearch(new OpenSearchConfig { Enabled = false })
        .WithRds(new RdsConfig { Enabled = false });

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    [InlineData("")]
    public void KeepsTheWaitStrategiesWhileTlsIsOff(string? tlsEnabled)
    {
        var cloud = new CloudDescriptor("x", 4577, "FLOCI_X_", "/_floci/health", null, Array.Empty<SocketService>());
        var env = new Dictionary<string, string>();
        if (tlsEnabled is not null)
        {
            env["FLOCI_X_TLS_ENABLED"] = tlsEnabled;
        }

        var current = DotNet.Testcontainers.Builders.Wait.ForUnixContainer().Build().ToArray();

        Assert.False(cloud.TlsEnabled(env));
        Assert.Same(current, FlociReadiness.WithHttpsWhenTlsEnabled(cloud, env, current));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    public void AddsAnHttpsWaitAfterTheConfiguredOnesWhenTlsIsOn(string tlsEnabled)
    {
        var cloud = new CloudDescriptor("x", 4577, "FLOCI_X_", "/_floci/health", null, Array.Empty<SocketService>());
        var env = new Dictionary<string, string> { ["FLOCI_X_TLS_ENABLED"] = tlsEnabled };
        var current = DotNet.Testcontainers.Builders.Wait.ForUnixContainer().Build().ToArray();

        var wanted = FlociReadiness.WithHttpsWhenTlsEnabled(cloud, env, current).ToArray();

        Assert.Equal("FLOCI_X_TLS_ENABLED", cloud.TlsEnabledEnv);
        Assert.True(cloud.TlsEnabled(env));
        Assert.True(wanted.Length > current.Length);
        Assert.Equal(current, wanted.Take(current.Length));
        // Another cloud's switch does not count.
        Assert.False(cloud.TlsEnabled(new Dictionary<string, string> { ["FLOCI_AZ_TLS_ENABLED"] = "true" }));
    }
}
