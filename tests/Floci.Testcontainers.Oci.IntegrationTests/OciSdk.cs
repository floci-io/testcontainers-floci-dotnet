using System;
using System.IO;
using Floci.Testcontainers.Oci;
using Oci.Common.Auth;
using Oci.ObjectstorageService;

namespace Floci.Testcontainers.Oci.Tests;

/// <summary>
/// Builds OCI SDK clients the way an application would: from a config file and a key file on
/// disk, both written from what the container hands out.
/// </summary>
internal static class OciSdk
{
    /// <param name="floci">The emulator.</param>
    /// <param name="profile">The profile name to write and load.</param>
    /// <param name="edit">Changes the config file contents before they are written.</param>
    public static ObjectStorageClient ObjectStorage(FlociOciContainer floci, string profile = "DEFAULT", Func<string, string>? edit = null)
    {
        var directory = Directory.CreateTempSubdirectory("floci-oci-").FullName;
        var keyFile = Path.Combine(directory, "floci_key.pem");
        var configFile = Path.Combine(directory, "config");
        var config = floci.GetConfigFileContents(keyFile, profile);
        File.WriteAllText(keyFile, floci.GetPrivateKeyPem());
        File.WriteAllText(configFile, edit is null ? config : edit(config));

        var client = new ObjectStorageClient(new ConfigFileAuthenticationDetailsProvider(configFile, profile));
        client.SetEndpoint(floci.GetEndpoint());

        // The client was built for the profile's region and remembers that region's endpoint
        // template, which it would otherwise prefer over the endpoint set above (seen with
        // OCI.DotNetSDK.Objectstorage 148.1.0: the requests went to Oracle's real endpoint).
        client.SetRealmSpecificEndpointTemplate(null);
        return client;
    }
}
