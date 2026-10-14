using System;
using System.Threading.Tasks;
using Floci.Testcontainers.Gcp;
using Xunit;

namespace Floci.Testcontainers.Gcp.Tests;

/// <summary>
/// One emulator shared by the tests of a class. While it runs, the test process carries the
/// variables of <see cref="FlociGcpContainer.GetEmulatorEnv" />, so clients built with emulator
/// detection reach it the way an application under test would. The suite runs serially, so the
/// process-wide variables never belong to two fixtures at once.
/// </summary>
public sealed class FlociGcpFixture : IAsyncLifetime
{
    public FlociGcpContainer Floci { get; } = new FlociGcpBuilder(TestImages.FlociGcp).Build();

    public async Task InitializeAsync()
    {
        await Floci.StartAsync();
        foreach (var variable in Floci.GetEmulatorEnv())
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
    }

    public async Task DisposeAsync()
    {
        foreach (var variable in Floci.GetEmulatorEnv())
        {
            Environment.SetEnvironmentVariable(variable.Key, null);
        }

        await Floci.DisposeAsync();
    }
}
