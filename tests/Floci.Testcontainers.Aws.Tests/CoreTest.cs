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

    [Fact]
    public void AReplacedConfigTakesItsSettingsWithIt()
    {
        var builder = new FlociBuilder(Image)
            .WithSqs(new SqsConfig { VisibilityTimeout = 60 })
            .WithRds(new RdsConfig { ProxyBasePort = 7010 })
            .WithSqs(new SqsConfig { Enabled = false });

        // Disabled, SQS emits its switch and nothing else; RDS is untouched.
        Assert.Equal(
            new[] { "FLOCI_SERVICES_SQS_ENABLED" },
            builder.Environments.Keys.Where(key => key.StartsWith("FLOCI_SERVICES_SQS_", StringComparison.Ordinal)));
        Assert.Equal("false", builder.Environments["FLOCI_SERVICES_SQS_ENABLED"]);
        Assert.Equal("7010", builder.Environments["FLOCI_SERVICES_RDS_PROXY_BASE_PORT"]);

        // Enabled again with defaults, the earlier custom value is gone too.
        Assert.Equal("30", builder.WithSqs(new SqsConfig()).Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
    }

    [Fact]
    public void AnExplicitVariableWinsOverAConfigWhicheverCameFirst()
    {
        var after = new FlociBuilder(Image)
            .WithSqs(new SqsConfig { VisibilityTimeout = 60 })
            .WithEnvironment("FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT", "5");
        var before = new FlociBuilder(Image)
            .WithEnvironment("FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT", "5")
            .WithSqs(new SqsConfig { VisibilityTimeout = 60 });

        Assert.Equal("5", after.Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
        Assert.Equal("5", before.Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);

        // A config applied later replaces the config, not what the caller set by hand.
        Assert.Equal("5", after.WithSqs(new SqsConfig { Enabled = false }).Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
    }

    [Fact]
    public void BuildWritesTheSurvivingConfigsToTheContainerConfiguration()
    {
        var written = new FlociBuilder(Image).WithSqs(new SqsConfig { VisibilityTimeout = 60 }).WithServiceSettings();

        Assert.Equal("60", written.Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
        // Once written they are container variables like any other, so a config no longer displaces them.
        Assert.Equal("60", written.WithSqs(new SqsConfig()).Environments["FLOCI_SERVICES_SQS_DEFAULT_VISIBILITY_TIMEOUT"]);
        // Writing twice changes nothing.
        Assert.Equal(written.Environments, written.WithServiceSettings().Environments);
    }

    [Fact]
    public void OnlyTheSurvivingEnabledConfigsPublishFixedPorts()
    {
        var configs = FlociServiceConfigs.Empty.With(new RdsConfig { ProxyBasePort = 7010, ProxyPortsCount = 2 });

        Assert.Equal(new[] { 7010, 7011 }, configs.FixedHostPorts);
        Assert.Equal(new[] { 7020, 7021 }, configs.With(new RdsConfig { ProxyBasePort = 7020, ProxyPortsCount = 2 }).FixedHostPorts);
        Assert.Empty(configs.With(new RdsConfig { Enabled = false }).FixedHostPorts);
        Assert.Empty(FlociServiceConfigs.Empty.FixedHostPorts);
    }

    [Fact]
    public void MergingConfigSetsReplacesServiceByService()
    {
        var older = FlociServiceConfigs.Empty.With(new SqsConfig { VisibilityTimeout = 60 }).With(new S3Config { Enabled = false });
        var newer = FlociServiceConfigs.Empty.With(new SqsConfig { Enabled = false });
        var none = new Dictionary<string, string>();

        Assert.Same(older, FlociServiceConfigs.Combine(older, FlociServiceConfigs.Empty));
        Assert.Equal(
            new Dictionary<string, string> { ["FLOCI_SERVICES_SQS_ENABLED"] = "false", ["FLOCI_SERVICES_S3_ENABLED"] = "false" },
            FlociServiceConfigs.Combine(older, newer).Environment(none));
        Assert.Same(none, FlociServiceConfigs.Empty.Environment(none));
    }
}
