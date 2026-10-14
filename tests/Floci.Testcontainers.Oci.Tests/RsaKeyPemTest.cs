using System.Security.Cryptography;
using Floci.Testcontainers.Oci;
using Xunit;

namespace Floci.Testcontainers.Oci.Tests;

public sealed class RsaKeyPemTest
{
    [Fact]
    public void GeneratesA2048BitKeyThePlatformCanReadBack()
    {
        var pem = RsaKeyPem.Generate();

        Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----\n", pem);
        Assert.EndsWith("-----END RSA PRIVATE KEY-----\n", pem);
        Assert.All(pem.Split('\n'), line => Assert.True(line.Length <= 64));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        Assert.Equal(2048, rsa.KeySize);

        // The imported key is whole: it signs, and its public half verifies.
        var data = new byte[] { 1, 2, 3 };
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Assert.True(rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void EncodesExactlyWhatThePlatformEncodes()
    {
        using var rsa = RSA.Create(2048);

        Assert.Equal(rsa.ExportRSAPrivateKeyPem() + "\n", RsaKeyPem.Encode(rsa.ExportParameters(true)));
    }

    [Fact]
    public void GeneratesADifferentKeyEachTime()
    {
        Assert.NotEqual(RsaKeyPem.Generate(), RsaKeyPem.Generate());
    }
}
