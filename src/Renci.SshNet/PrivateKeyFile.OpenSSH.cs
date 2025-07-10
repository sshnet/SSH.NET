#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;

using CipherMode = System.Security.Cryptography.CipherMode;

namespace Renci.SshNet
{
    public partial class PrivateKeyFile
    {
        private sealed class OpenSSH : IPrivateKeyParser
        {
            private readonly byte[] _data;
            private readonly string? _passPhrase;

            public OpenSSH(byte[] data, string? passPhrase)
            {
                _data = data;
                _passPhrase = passPhrase;
            }

            /// <summary>
            /// Parses an OpenSSH V1 key file according to the key spec:
            /// <see href="https://github.com/openssh/openssh-portable/blob/master/PROTOCOL.key"/>.
            /// </summary>
            public Key Parse()
            {
                var keyReader = new SshDataStream(_data);

                // check magic header
                var authMagic = "openssh-key-v1\0"u8;
                var keyHeaderBytes = keyReader.ReadBytes(authMagic.Length);
                if (!authMagic.SequenceEqual(keyHeaderBytes))
                {
                    throw new SshException("This openssh key does not contain the 'openssh-key-v1' format magic header");
                }

                // cipher will be "aes256-cbc" or other cipher if using a passphrase, "none" otherwise
                var cipherName = keyReader.ReadString(Encoding.UTF8);

                // key derivation function (kdf): bcrypt or nothing
                var kdfName = keyReader.ReadString(Encoding.UTF8);

                // kdf options length: 24 if passphrase, 0 if no passphrase
                var kdfOptionsLen = (int)keyReader.ReadUInt32();
                byte[]? salt = null;
                var rounds = 0;
                if (kdfOptionsLen > 0)
                {
                    var saltLength = (int)keyReader.ReadUInt32();
                    salt = keyReader.ReadBytes(saltLength);
                    rounds = (int)keyReader.ReadUInt32();
                }

                // number of public keys, only supporting 1 for now
                var numberOfPublicKeys = (int)keyReader.ReadUInt32();
                if (numberOfPublicKeys != 1)
                {
                    throw new SshException("At this time only one public key in the openssh key is supported.");
                }

                // read public key in ssh-format, but we don't need it
                _ = keyReader.ReadString(Encoding.UTF8);

                // possibly encrypted private key
                var privateKeyLength = (int)keyReader.ReadUInt32();
                byte[] privateKeyBytes;

                // decrypt private key if necessary
                if (cipherName != "none")
                {
                    if (string.IsNullOrEmpty(_passPhrase))
                    {
                        throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                    }

                    if (string.IsNullOrEmpty(kdfName) || kdfName != "bcrypt")
                    {
                        throw new SshException("kdf " + kdfName + " is not supported for openssh key file");
                    }

                    var ivLength = 16;
                    CipherInfo cipherInfo;
                    switch (cipherName)
                    {
                        case "3des-cbc":
                            ivLength = 8;
                            cipherInfo = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, iv, CipherMode.CBC, pkcs7Padding: false));
                            break;
                        case "aes128-cbc":
                            cipherInfo = new CipherInfo(128, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: false));
                            break;
                        case "aes192-cbc":
                            cipherInfo = new CipherInfo(192, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: false));
                            break;
                        case "aes256-cbc":
                            cipherInfo = new CipherInfo(256, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: false));
                            break;
                        case "aes128-ctr":
                            cipherInfo = new CipherInfo(128, (key, iv) => new AesCipher(key, iv, AesCipherMode.CTR, pkcs7Padding: false));
                            break;
                        case "aes192-ctr":
                            cipherInfo = new CipherInfo(192, (key, iv) => new AesCipher(key, iv, AesCipherMode.CTR, pkcs7Padding: false));
                            break;
                        case "aes256-ctr":
                            cipherInfo = new CipherInfo(256, (key, iv) => new AesCipher(key, iv, AesCipherMode.CTR, pkcs7Padding: false));
                            break;
                        case "aes128-gcm@openssh.com":
                            cipherInfo = new CipherInfo(128, (key, iv) => new AesGcmCipher(key, iv, aadLength: 0), isAead: true);
                            break;
                        case "aes256-gcm@openssh.com":
                            cipherInfo = new CipherInfo(256, (key, iv) => new AesGcmCipher(key, iv, aadLength: 0), isAead: true);
                            break;
                        case "chacha20-poly1305@openssh.com":
                            ivLength = 12;
                            cipherInfo = new CipherInfo(256, (key, iv) => new ChaCha20Poly1305Cipher(key, aadLength: 0), isAead: true);
                            break;
                        default:
                            throw new SshException("Cipher '" + cipherName + "' is not supported for an OpenSSH key.");
                    }

                    var keyLength = cipherInfo.KeySize / 8;

                    // inspired by the SSHj library (https://github.com/hierynomus/sshj)
                    // apply the kdf to derive a key and iv from the passphrase
                    var passPhraseBytes = Encoding.UTF8.GetBytes(_passPhrase);
                    var keyiv = new byte[keyLength + ivLength];
                    new BCrypt().Pbkdf(passPhraseBytes, salt, rounds, keyiv);

                    var key = keyiv.Take(keyLength);
                    var iv = keyiv.Take(keyLength, ivLength);

                    var cipher = cipherInfo.Cipher(key, iv);

                    // The authentication tag data (if any) is concatenated to the end of the encrypted private key string.
                    // See https://github.com/openssh/openssh-portable/blob/509b757c052ea969b3a41fc36818b44801caf1cf/sshkey.c#L2951
                    // and https://github.com/openssh/openssh-portable/blob/509b757c052ea969b3a41fc36818b44801caf1cf/cipher.c#L340
                    var cipherData = keyReader.ReadBytes(privateKeyLength + cipher.TagSize);

                    try
                    {
                        privateKeyBytes = cipher.Decrypt(cipherData, 0, privateKeyLength);
                    }
                    finally
                    {
                        if (cipher is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
                else
                {
                    privateKeyBytes = keyReader.ReadBytes(privateKeyLength);
                }

                // validate private key length
                privateKeyLength = privateKeyBytes.Length;
                if (privateKeyLength % 8 != 0)
                {
                    throw new SshException("The private key section must be a multiple of the block size (8)");
                }

                // now parse the data we called the private key, it actually contains the public key again
                // so we need to parse through it to get the private key bytes, plus there's some
                // validation we need to do.
                var privateKeyReader = new SshDataStream(privateKeyBytes);

                // check ints should match, they wouldn't match for example if the wrong passphrase was supplied
                var checkInt1 = (int)privateKeyReader.ReadUInt32();
                var checkInt2 = (int)privateKeyReader.ReadUInt32();
                if (checkInt1 != checkInt2)
                {
                    throw new SshException(string.Format(CultureInfo.InvariantCulture,
                                                         "The random check bytes of the OpenSSH key do not match ({0} <-> {1}).",
                                                         checkInt1.ToString(CultureInfo.InvariantCulture),
                                                         checkInt2.ToString(CultureInfo.InvariantCulture)));
                }

                // key type
                var keyType = privateKeyReader.ReadString(Encoding.UTF8);

                Key parsedKey;
                byte[] publicKey;
                byte[] unencryptedPrivateKey;
                switch (keyType)
                {
                    case "ssh-ed25519":
                        // https://datatracker.ietf.org/doc/html/draft-miller-ssh-agent-11#section-3.2.3

                        // ENC(A)
                        _ = privateKeyReader.ReadBinarySegment();

                        // k || ENC(A)
                        unencryptedPrivateKey = privateKeyReader.ReadBinary();
                        parsedKey = new ED25519Key(unencryptedPrivateKey);
                        break;
                    case "ecdsa-sha2-nistp256":
                    case "ecdsa-sha2-nistp384":
                    case "ecdsa-sha2-nistp521":
                        var curve = privateKeyReader.ReadString(Encoding.ASCII);

                        publicKey = privateKeyReader.ReadBinary();

                        unencryptedPrivateKey = privateKeyReader.ReadBinary();
                        parsedKey = new EcdsaKey(curve, publicKey, unencryptedPrivateKey.TrimLeadingZeros());
                        break;
                    case "ssh-rsa":
                        var modulus = privateKeyReader.ReadBigInt();
                        var exponent = privateKeyReader.ReadBigInt();
                        var d = privateKeyReader.ReadBigInt();
                        var inverseQ = privateKeyReader.ReadBigInt();
                        var p = privateKeyReader.ReadBigInt();
                        var q = privateKeyReader.ReadBigInt();
                        parsedKey = new RsaKey(modulus, exponent, d, p, q, inverseQ);
                        break;
                    default:
                        throw new SshException("OpenSSH key type '" + keyType + "' is not supported.");
                }

                parsedKey.Comment = privateKeyReader.ReadString(Encoding.UTF8);

                // The list of privatekey/comment pairs is padded with the bytes 1, 2, 3, ...
                // until the total length is a multiple of the cipher block size.
                int b, i = 0;

                while ((b = privateKeyReader.ReadByte()) != -1)
                {
                    if (b != i + 1)
                    {
                        throw new SshException("Padding of openssh key format contained wrong byte at position: " +
                                               i.ToString(CultureInfo.InvariantCulture));
                    }

                    i++;
                }

                return parsedKey;
            }
        }
    }
}
