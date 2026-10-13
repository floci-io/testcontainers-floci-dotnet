using Floci.Testcontainers.Az;
using Xunit;

namespace Floci.Testcontainers.Az.Tests;

public sealed class SqlConfigTest
{
    [Fact]
    public void DefaultsMatchUpstream()
    {
        var config = new SqlConfig();

        Assert.True(config.Enabled);
        Assert.Null(config.DataPlaneProvider);
        Assert.Equal("N", config.AcceptEula);
        Assert.Equal(SqlDataPlaneProvider.None, config.EffectiveDataPlaneProvider);
        Assert.False(config.RequiresDockerAccess);
    }

    [Fact]
    public void DefaultConfigEmitsUpstreamDefaultEnvVars()
    {
        var env = new SqlConfig().BuildEnvironment();

        Assert.Equal("true", env["FLOCI_AZ_SERVICES_SQL_ENABLED"]);
        Assert.Equal("N", env["FLOCI_AZ_SERVICES_SQL_ACCEPT_EULA"]);
        Assert.DoesNotContain("FLOCI_AZ_SERVICES_SQL_DATA_PLANE_PROVIDER", env.Keys);
        Assert.Equal(2, env.Count);
    }

    [Theory]
    [InlineData(SqlDataPlaneProvider.None, "none")]
    [InlineData(SqlDataPlaneProvider.Managed, "managed")]
    [InlineData(SqlDataPlaneProvider.External, "external")]
    public void CustomConfigEmitsCustomEnvVars(SqlDataPlaneProvider provider, string expected)
    {
        var env = new SqlConfig { AcceptEula = "Y", DataPlaneProvider = provider }.BuildEnvironment();

        Assert.Equal("Y", env["FLOCI_AZ_SERVICES_SQL_ACCEPT_EULA"]);
        Assert.Equal(expected, env["FLOCI_AZ_SERVICES_SQL_DATA_PLANE_PROVIDER"]);
    }

    [Fact]
    public void RequiresDockerAccessOnlyWithTheManagedDataPlane()
    {
        Assert.True(new SqlConfig { AcceptEula = "Y" }.RequiresDockerAccess);
        Assert.True(new SqlConfig { AcceptEula = "y" }.RequiresDockerAccess);
        Assert.True(new SqlConfig { DataPlaneProvider = SqlDataPlaneProvider.Managed }.RequiresDockerAccess);
        Assert.False(new SqlConfig { AcceptEula = "Y", DataPlaneProvider = SqlDataPlaneProvider.None }.RequiresDockerAccess);
        Assert.False(new SqlConfig { AcceptEula = "Y", DataPlaneProvider = SqlDataPlaneProvider.External }.RequiresDockerAccess);
    }
}
