#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;

using CipherMode = System.Security.Cryptography.CipherMode;

namespace Renci.SshNet
{
    public partial class PrivateKeyFile
    {
        private sealed class SSHCOM : IPrivateKeyParser
        {
            private readonly byte[] _data;
            private readonly string? _passPhrase;

            public SSHCOM(byte[] data, string? passPhrase)
            {
                _data = data;
                _passPhrase = passPhrase;
            }

            public Key Parse()
            {
                var reader = new SshDataStream(_data);
                var magicNumber = reader.ReadUInt32();
                if (magicNumber != 0x3f6ff9eb)
                {
                    throw new SshException("Invalid SSH2 private key.");
                }

                _ = reader.ReadUInt32(); // Read total bytes length including magic number
                var keyType = reader.ReadString(SshData.Ascii);
                var ssh2CipherName = reader.ReadString(SshData.Ascii);
                var blobSize = (int)reader.ReadUInt32();

                byte[] keyData;
                if (ssh2CipherName == "none")
                {
                    keyData = reader.ReadBytes(blobSize);
                }
                else if (ssh2CipherName == "3des-cbc")
                {
                    if (string.IsNullOrEmpty(_passPhrase))
                    {
                        throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                    }

                    var key = GetCipherKey(_passPhrase, 192 / 8);
                    var ssh2Сipher = new TripleDesCipher(key, new byte[8], CipherMode.CBC, pkcs7Padding: false);
                    keyData = ssh2Сipher.Decrypt(reader.ReadBytes(blobSize));
                }
                else
                {
                    throw new SshException(string.Format("Cipher method '{0}' is not supported.", ssh2CipherName));
                }

                reader = new SshDataStream(keyData);

                var decryptedLength = reader.ReadUInt32();

                if (decryptedLength > blobSize - 4)
                {
                    throw new SshException("Invalid passphrase.");
                }

                if (keyType.Contains("rsa"))
                {
                    var exponent = ReadBigIntWithBits(reader);
                    var d = ReadBigIntWithBits(reader);
                    var modulus = ReadBigIntWithBits(reader);
                    var inverseQ = ReadBigIntWithBits(reader);
                    var q = ReadBigIntWithBits(reader);
                    var p = ReadBigIntWithBits(reader);
                    return new RsaKey(modulus, exponent, d, p, q, inverseQ);
                }

                throw new NotSupportedException(string.Format("Key type '{0}' is not supported.", keyType));

                // Reads next mpint where length is specified in bits.
                static BigInteger ReadBigIntWithBits(SshDataStream reader)
                {
                    var numBits = (int)reader.ReadUInt32();

                    var numBytes = (numBits + 7) / 8;

                    return reader.ReadBytes(numBytes).ToBigInteger2();
                }
            }

            private static byte[] GetCipherKey(string passphrase, int length)
            {
                var cipherKey = new List<byte>();

#pragma warning disable CA1850 // Prefer static HashData method; We'll reuse the object on lower targets.
                using (var md5 = MD5.Create())
                {
                    var passwordBytes = Encoding.UTF8.GetBytes(passphrase);

                    var hash = md5.ComputeHash(passwordBytes);
                    cipherKey.AddRange(hash);

                    while (cipherKey.Count < length)
                    {
                        hash = passwordBytes.Concat(hash);
                        hash = md5.ComputeHash(hash);
                        cipherKey.AddRange(hash);
                    }
                }
#pragma warning restore CA1850 // Prefer static HashData method

                return cipherKey.ToArray().Take(length);
            }
        }
    }
}
