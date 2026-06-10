using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class Route53ConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new Route53Config();

        Assert.True(config.Enabled);
        Assert.Equal("ns-1.awsdns-01.org", config.DefaultNameserver1);
        Assert.Equal("ns-2.awsdns-02.net", config.DefaultNameserver2);
        Assert.Equal("ns-3.awsdns-03.com", config.DefaultNameserver3);
        Assert.Equal("ns-4.awsdns-04.co.uk", config.DefaultNameserver4);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new Route53Config().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ROUTE53_ENABLED"]);
        Assert.Equal("ns-1.awsdns-01.org", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_1"]);
        Assert.Equal("ns-2.awsdns-02.net", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_2"]);
        Assert.Equal("ns-3.awsdns-03.com", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_3"]);
        Assert.Equal("ns-4.awsdns-04.co.uk", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_4"]);
    }

    [Fact]
    public void CustomConfigEmitsCustomEnvVars()
    {
        var env = new Route53Config
        {
            DefaultNameserver1 = "ns1.example.com",
            DefaultNameserver2 = "ns2.example.com",
            DefaultNameserver3 = "ns3.example.com",
            DefaultNameserver4 = "ns4.example.com",
        }.BuildEnvironment();

        Assert.Equal("true", env["FLOCI_SERVICES_ROUTE53_ENABLED"]);
        Assert.Equal("ns1.example.com", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_1"]);
        Assert.Equal("ns2.example.com", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_2"]);
        Assert.Equal("ns3.example.com", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_3"]);
        Assert.Equal("ns4.example.com", env["FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_4"]);
    }

    [Fact]
    public void DisabledConfigEmitsOnlyTheEnabledFlag()
    {
        var env = new Route53Config { Enabled = false }.BuildEnvironment();

        Assert.Equal("false", env["FLOCI_SERVICES_ROUTE53_ENABLED"]);
        Assert.DoesNotContain("FLOCI_SERVICES_ROUTE53_DEFAULT_NAMESERVER_1", env.Keys);
    }
}
