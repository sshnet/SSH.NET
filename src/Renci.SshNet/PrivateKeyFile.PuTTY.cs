#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet
{
    public partial class PrivateKeyFile
    {
        private sealed class PuTTY : IPrivateKeyParser
        {
            private readonly string _version;
            private readonly string _algorithmName;
            private readonly string _encryptionType;
            private readonly string _comment;
            private readonly byte[] _publicKey;
            private readonly string? _argon2Type;
            private readonly string? _argon2Salt;
            private readonly string? _argon2Iterations;
            private readonly string? _argon2Memory;
            private readonly string? _argon2Parallelism;
            private readonly byte[] _data;
            private readonly string _mac;
            private readonly string? _passPhrase;

            public PuTTY(string version, string algorithmName, string encryptionType, string comment, byte[] publicKey, string? argon2Type, string? argon2Salt, string? argon2Iterations, string? argon2Memory, string? argon2Parallelism, byte[] data, string mac, string? passPhrase)
            {
                _version = version;
                _algorithmName = algorithmName;
                _encryptionType = encryptionType;
                _comment = comment;
                _publicKey = publicKey;
                _argon2Type = argon2Type;
                _argon2Salt = argon2Salt;
                _argon2Iterations = argon2Iterations;
                _argon2Memory = argon2Memory;
                _argon2Parallelism = argon2Parallelism;
                _data = data;
                _mac = mac;
                _passPhrase = passPhrase;
            }

            /// <summary>
            /// Parses an PuTTY PPK key file.
            /// <see href="https://tartarus.org/~simon/putty-snapshots/htmldoc/AppendixC.html"/>.
            /// </summary>
            public Key Parse()
            {
                byte[] privateKey;
                HMAC hmac;
                switch (_encryptionType)
                {
                    case "aes256-cbc":
                        if (string.IsNullOrEmpty(_passPhrase))
                        {
                            throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                        }

                        byte[] cipherKey;
                        byte[] cipherIV;
                        switch (_version)
                        {
                            case "3":
                                ThrowHelper.ThrowIfNullOrEmpty(_argon2Type);
                                ThrowHelper.ThrowIfNullOrEmpty(_argon2Iterations);
                                ThrowHelper.ThrowIfNullOrEmpty(_argon2Memory);
                                ThrowHelper.ThrowIfNullOrEmpty(_argon2Parallelism);
                                ThrowHelper.ThrowIfNullOrEmpty(_argon2Salt);

                                var keyData = Argon2(
                                    _argon2Type,
                                    Convert.ToInt32(_argon2Iterations),
                                    Convert.ToInt32(_argon2Memory),
                                    Convert.ToInt32(_argon2Parallelism),
#if NET
                                    Convert.FromHexString(_argon2Salt),
#else
                                    Org.BouncyCastle.Utilities.Encoders.Hex.Decode(_argon2Salt),
#endif
                                    _passPhrase);

                                cipherKey = keyData.Take(32);
                                cipherIV = keyData.Take(32, 16);

                                var macKey = keyData.Take(48, 32);
                                hmac = new HMACSHA256(macKey);

                                break;
                            case "2":
                                keyData = V2KDF(_passPhrase);

                                cipherKey = keyData.Take(32);
                                cipherIV = new byte[16];

                                macKey = CryptoAbstraction.HashSHA1(Encoding.UTF8.GetBytes("putty-private-key-file-mac-key" + _passPhrase)).Take(20);
                                hmac = new HMACSHA1(macKey);

                                break;
                            default:
                                throw new SshException("PuTTY key file version " + _version + " is not supported");
                        }

                        using (var cipher = new AesCipher(cipherKey, cipherIV, AesCipherMode.CBC, pkcs7Padding: false))
                        {
                            privateKey = cipher.Decrypt(_data);
                        }

                        break;
                    case "none":
                        switch (_version)
                        {
                            case "3":
                                hmac = new HMACSHA256(Array.Empty<byte>());
                                break;
                            case "2":
                                var macKey = CryptoAbstraction.HashSHA1(Encoding.UTF8.GetBytes("putty-private-key-file-mac-key"));
                                hmac = new HMACSHA1(macKey);
                                break;
                            default:
                                throw new SshException("PuTTY key file version " + _version + " is not supported");
                        }

                        privateKey = _data;
                        break;
                    default:
                        throw new SshException("Encryption " + _encryptionType + " is not supported for PuTTY key file");
                }

                byte[] macData;
                using (var macStream = new SshDataStream(256))
                {
                    macStream.Write(_algorithmName, Encoding.UTF8);
                    macStream.Write(_encryptionType, Encoding.UTF8);
                    macStream.Write(_comment, Encoding.UTF8);
                    macStream.WriteBinary(_publicKey);
                    macStream.WriteBinary(privateKey);
                    macData = macStream.ToArray();
                }

                byte[] macValue;
                using (hmac)
                {
                    macValue = hmac.ComputeHash(macData);
                }
#if NET
                var reference = Convert.FromHexString(_mac);
#else
                var reference = Org.BouncyCastle.Utilities.Encoders.Hex.Decode(_mac);
#endif
                if (!macValue.SequenceEqual(reference))
                {
                    throw new SshException("MAC verification failed for PuTTY key file");
                }

                var publicKeyReader = new SshDataStream(_publicKey);
                var keyType = publicKeyReader.ReadString(Encoding.UTF8);
                Debug.Assert(keyType == _algorithmName, $"{nameof(keyType)} is not the same as {nameof(_algorithmName)}");

                var privateKeyReader = new SshDataStream(privateKey);

                Key parsedKey;

                switch (keyType)
                {
                    case "ssh-ed25519":
                        parsedKey = new ED25519Key(privateKeyReader.ReadBinary());
                        break;
                    case "ecdsa-sha2-nistp256":
                    case "ecdsa-sha2-nistp384":
                    case "ecdsa-sha2-nistp521":
                        var curve = publicKeyReader.ReadString(Encoding.ASCII);
                        var pub = publicKeyReader.ReadBinary();
                        var prv = privateKeyReader.ReadBinary();
                        parsedKey = new EcdsaKey(curve, pub, prv);
                        break;
                    case "ssh-rsa":
                        var exponent = publicKeyReader.ReadBigInt();
                        var modulus = publicKeyReader.ReadBigInt();
                        var d = privateKeyReader.ReadBigInt();
                        var p = privateKeyReader.ReadBigInt();
                        var q = privateKeyReader.ReadBigInt();
                        var inverseQ = privateKeyReader.ReadBigInt();
                        parsedKey = new RsaKey(modulus, exponent, d, p, q, inverseQ);
                        break;
                    default:
                        throw new SshException("Key type " + keyType + " is not supported for PuTTY key file");
                }

                parsedKey.Comment = _comment;
                return parsedKey;
            }

            private static byte[] Argon2(string type, int iterations, int memory, int parallelism, byte[] salt, string passPhrase)
            {
                int param;
                switch (type)
                {
                    case "Argon2i":
                        param = Argon2Parameters.Argon2i;
                        break;
                    case "Argon2d":
                        param = Argon2Parameters.Argon2d;
                        break;
                    case "Argon2id":
                        param = Argon2Parameters.Argon2id;
                        break;
                    default:
                        throw new SshException("KDF " + type + " is not supported for PuTTY key file");
                }

                var a2p = new Argon2Parameters.Builder(param)
                    .WithVersion(Argon2Parameters.Version13)
                    .WithIterations(iterations)
                    .WithMemoryAsKB(memory)
                    .WithParallelism(parallelism)
                    .WithSalt(salt).Build();

                var generator = new Argon2BytesGenerator();

                generator.Init(a2p);

                var output = new byte[80];
                var bytes = generator.GenerateBytes(passPhrase.ToCharArray(), output);

                if (bytes != output.Length)
                {
                    throw new SshException("Failed to generate key via Argon2");
                }

                return output;
            }

            private static byte[] V2KDF(string passPhrase)
            {
                var cipherKey = new List<byte>();

                var passPhraseBytes = Encoding.UTF8.GetBytes(passPhrase);
                for (var sequenceNumber = 0; sequenceNumber < 2; sequenceNumber++)
                {
                    using (var sha1 = SHA1.Create())
                    {
                        var sequence = new byte[] { 0, 0, 0, (byte)sequenceNumber };
                        _ = sha1.TransformBlock(sequence, 0, 4, outputBuffer: null, 0);
                        _ = sha1.TransformFinalBlock(passPhraseBytes, 0, passPhraseBytes.Length);
                        Debug.Assert(sha1.Hash != null, "Hash is null");
                        cipherKey.AddRange(sha1.Hash);
                    }
                }

                return cipherKey.ToArray();
            }
        }
    }
}
