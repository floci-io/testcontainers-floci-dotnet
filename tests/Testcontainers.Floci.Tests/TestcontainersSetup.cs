using System;
using System.Runtime.CompilerServices;

namespace Testcontainers.Floci.Tests;

internal static class TestcontainersSetup
{
    /// <summary>
    /// Colima exposes the Docker socket over virtiofs, which cannot be bind-mounted into a
    /// container. The Testcontainers resource reaper (Ryuk) requires that bind mount, so on
    /// Colima it fails container creation. We disable Ryuk unless the environment already
    /// expresses a preference, so local runs work out of the box while CI (with a real Docker
    /// daemon) can re-enable it by setting TESTCONTAINERS_RYUK_DISABLED=false.
    /// </summary>
    [ModuleInitializer]
    internal static void Init()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED")))
        {
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        }
    }
}
