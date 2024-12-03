#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;

namespace Renci.SshNet
{
    public partial class PrivateKeyFile
    {
        private sealed class PKCS1 : IPrivateKeyParser
        {
            private readonly string _cipherName;
            private readonly string _salt;
            private readonly string _keyName;
            private readonly byte[] _data;
            private readonly string? _passPhrase;

            public PKCS1(string cipherName, string salt, string keyName, byte[] data, string? passPhrase)
            {
                _cipherName = cipherName;
                _salt = salt;
                _keyName = keyName;
                _data = data;
                _passPhrase = passPhrase;
            }

            public Key Parse()
            {
                byte[] decryptedData;
                if (!string.IsNullOrEmpty(_cipherName) && !string.IsNullOrEmpty(_salt))
                {
                    if (string.IsNullOrEmpty(_passPhrase))
                    {
                        throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                    }
#if NET
                    var binarySalt = Convert.FromHexString(_salt);
#else
                    var binarySalt = Org.BouncyCastle.Utilities.Encoders.Hex.Decode(_salt);
#endif
                    CipherInfo cipher;
                    switch (_cipherName)
                    {
                        case "DES-EDE3-CBC":
                            cipher = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                            break;
                        case "DES-EDE3-CFB":
                            cipher = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CfbCipherMode(iv), padding: null));
                            break;
                        case "DES-CBC":
                            cipher = new CipherInfo(64, (key, iv) => new DesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                            break;
                        case "AES-128-CBC":
                            cipher = new CipherInfo(128, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: true));
                            break;
                        case "AES-192-CBC":
                            cipher = new CipherInfo(192, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: true));
                            break;
                        case "AES-256-CBC":
                            cipher = new CipherInfo(256, (key, iv) => new AesCipher(key, iv, AesCipherMode.CBC, pkcs7Padding: true));
                            break;
                        default:
                            throw new SshException(string.Format(CultureInfo.InvariantCulture, "Private key cipher \"{0}\" is not supported.", _cipherName));
                    }

                    decryptedData = DecryptKey(cipher, _data, _passPhrase, binarySalt);
                }
                else
                {
                    decryptedData = _data;
                }

                switch (_keyName)
                {
                    case "RSA PRIVATE KEY":
                        return new RsaKey(decryptedData);
                    case "DSA PRIVATE KEY":
                        return new DsaKey(decryptedData);
                    case "EC PRIVATE KEY":
                        return new EcdsaKey(decryptedData);
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", _keyName));
                }
            }

            /// <summary>
            /// Decrypts encrypted private key file data.
            /// </summary>
            /// <param name="cipherInfo">The cipher info.</param>
            /// <param name="cipherData">Encrypted data.</param>
            /// <param name="passPhrase">Decryption pass phrase.</param>
            /// <param name="binarySalt">Decryption binary salt.</param>
            /// <returns>Decrypted byte array.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="cipherInfo" />, <paramref name="cipherData" />, <paramref name="passPhrase" /> or <paramref name="binarySalt" /> is <see langword="null"/>.</exception>
            private static byte[] DecryptKey(CipherInfo cipherInfo, byte[] cipherData, string passPhrase, byte[] binarySalt)
            {
                Debug.Assert(cipherInfo != null);
                Debug.Assert(cipherData != null);
                Debug.Assert(binarySalt != null);

                var cipherKey = new List<byte>();

#pragma warning disable CA1850 // Prefer static HashData method; We'll reuse the object on lower targets.
                using (var md5 = MD5.Create())
                {
                    var passwordBytes = Encoding.UTF8.GetBytes(passPhrase);

                    // Use 8 bytes binary salt
                    var initVector = passwordBytes.Concat(binarySalt.Take(8));

                    var hash = md5.ComputeHash(initVector);
                    cipherKey.AddRange(hash);

                    while (cipherKey.Count < cipherInfo.KeySize / 8)
                    {
                        hash = hash.Concat(initVector);
                        hash = md5.ComputeHash(hash);
                        cipherKey.AddRange(hash);
                    }
                }
#pragma warning restore CA1850 // Prefer static HashData method

                var cipher = cipherInfo.Cipher(cipherKey.ToArray(), binarySalt);

                try
                {
                    return cipher.Decrypt(cipherData);
                }
                finally
                {
                    if (cipher is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
