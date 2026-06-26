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
                using var dataReader = new SshDataStream(_data);
                var magicNumber = dataReader.ReadUInt32();
                if (magicNumber != 0x3f6ff9eb)
                {
                    throw new SshException("Invalid SSH2 private key.");
                }

                _ = dataReader.ReadUInt32(); // Read total bytes length including magic number
                var keyType = dataReader.ReadString(SshData.Ascii);
                var ssh2CipherName = dataReader.ReadString(SshData.Ascii);
                var blobSize = (int)dataReader.ReadUInt32();

                byte[] keyData;
                if (ssh2CipherName == "none")
                {
                    keyData = dataReader.ReadBytes(blobSize);
                }
                else if (ssh2CipherName == "3des-cbc")
                {
                    if (string.IsNullOrEmpty(_passPhrase))
                    {
                        throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                    }

                    var key = GetCipherKey(_passPhrase, 192 / 8);
                    using var ssh2Сipher = new TripleDesCipher(key, new byte[8], CipherMode.CBC, pkcs7Padding: false);
                    keyData = ssh2Сipher.Decrypt(dataReader.ReadBytes(blobSize));
                }
                else
                {
                    throw new SshException(string.Format("Cipher method '{0}' is not supported.", ssh2CipherName));
                }

                using var keyReader = new SshDataStream(keyData);

                var decryptedLength = keyReader.ReadUInt32();

                if (decryptedLength > blobSize - 4)
                {
                    throw new SshException("Invalid passphrase.");
                }

                if (keyType.Contains("rsa", StringComparison.Ordinal))
                {
                    var exponent = ReadBigIntWithBits(keyReader);
                    var d = ReadBigIntWithBits(keyReader);
                    var modulus = ReadBigIntWithBits(keyReader);
                    var inverseQ = ReadBigIntWithBits(keyReader);
                    var q = ReadBigIntWithBits(keyReader);
                    var p = ReadBigIntWithBits(keyReader);
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
