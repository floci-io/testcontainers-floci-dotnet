using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Floci.Testcontainers.Oci;

/// <summary>
/// Generates a throwaway RSA key as PEM. Written by hand because <c>netstandard2.0</c> has no
/// key export beyond <see cref="RSAParameters" />.
/// </summary>
internal static class RsaKeyPem
{
    /// <summary>Generates a 2048-bit RSA private key.</summary>
    /// <returns>The key as a PKCS#1 PEM document (<c>BEGIN RSA PRIVATE KEY</c>).</returns>
    public static string Generate()
    {
        using var rsa = RSA.Create();
        rsa.KeySize = 2048;
        return Encode(rsa.ExportParameters(includePrivateParameters: true));
    }

    /// <summary>Encodes a private key as a PKCS#1 <c>RSAPrivateKey</c> PEM document.</summary>
    /// <param name="key">The key, including its private parameters.</param>
    /// <returns>The PEM document.</returns>
    internal static string Encode(RSAParameters key)
    {
        var body = new List<byte>();
        foreach (var value in new[] { new byte[] { 0 }, key.Modulus, key.Exponent, key.D, key.P, key.Q, key.DP, key.DQ, key.InverseQ })
        {
            body.AddRange(Integer(value!));
        }

        var base64 = Convert.ToBase64String(Tlv(0x30, body));
        var pem = new StringBuilder("-----BEGIN RSA PRIVATE KEY-----\n");
        for (var offset = 0; offset < base64.Length; offset += 64)
        {
            pem.Append(base64, offset, Math.Min(64, base64.Length - offset)).Append('\n');
        }

        return pem.Append("-----END RSA PRIVATE KEY-----\n").ToString();
    }

    /// <summary>A DER INTEGER from an unsigned big-endian value.</summary>
    private static byte[] Integer(byte[] unsigned)
    {
        var start = 0;
        while (start < unsigned.Length - 1 && unsigned[start] == 0)
        {
            start++;
        }

        var content = new List<byte>();
        if ((unsigned[start] & 0x80) != 0)
        {
            // Keep the value positive.
            content.Add(0);
        }

        for (var index = start; index < unsigned.Length; index++)
        {
            content.Add(unsigned[index]);
        }

        return Tlv(0x02, content);
    }

    private static byte[] Tlv(byte tag, List<byte> content)
    {
        var der = new List<byte> { tag };
        if (content.Count < 0x80)
        {
            der.Add((byte)content.Count);
        }
        else if (content.Count <= 0xFF)
        {
            der.Add(0x81);
            der.Add((byte)content.Count);
        }
        else
        {
            der.Add(0x82);
            der.Add((byte)(content.Count >> 8));
            der.Add((byte)(content.Count & 0xFF));
        }

        der.AddRange(content);
        return der.ToArray();
    }
}
