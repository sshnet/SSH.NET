using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Pkcs;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following private keys are supported:
    /// <list type="bullet">
    ///     <item>
    ///         <description>RSA in OpenSSL PEM, ssh.com and OpenSSH key format</description>
    ///     </item>
    ///     <item>
    ///         <description>DSA in OpenSSL PEM and ssh.com format</description>
    ///     </item>
    ///     <item>
    ///         <description>ECDSA 256/384/521 in OpenSSL PEM and OpenSSH key format</description>
    ///     </item>
    ///     <item>
    ///         <description>ED25519 in OpenSSL PEM and OpenSSH key format</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The following encryption algorithms are supported for OpenSSL traditional PEM:
    /// <list type="bullet">
    ///     <item>
    ///         <description>DES-EDE3-CBC</description>
    ///     </item>
    ///     <item>
    ///         <description>DES-EDE3-CFB</description>
    ///     </item>
    ///     <item>
    ///         <description>DES-CBC</description>
    ///     </item>
    ///     <item>
    ///         <description>AES-128-CBC</description>
    ///     </item>
    ///     <item>
    ///         <description>AES-192-CBC</description>
    ///     </item>
    ///     <item>
    ///         <description>AES-256-CBC</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// Private keys in OpenSSL PKCS#8 PEM format can be encrypted using any cipher method BouncyCastle supports.
    /// </para>
    /// <para>
    /// The following encryption algorithms are supported for ssh.com format:
    /// <list type="bullet">
    ///     <item>
    ///         <description>3des-cbc</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The following encryption algorithms are supported for OpenSSH format:
    /// <list type="bullet">
    ///     <item>
    ///         <description>3des-cbc</description>
    ///     </item>
    ///     <item>
    ///         <description>aes128-cbc</description>
    ///     </item>
    ///     <item>
    ///         <description>aes192-cbc</description>
    ///     </item>
    ///     <item>
    ///         <description>aes256-cbc</description>
    ///     </item>
    ///     <item>
    ///         <description>aes128-ctr</description>
    ///     </item>
    ///     <item>
    ///         <description>aes192-ctr</description>
    ///     </item>
    ///     <item>
    ///         <description>aes256-ctr</description>
    ///     </item>
    ///     <item>
    ///         <description>aes128-gcm@openssh.com</description>
    ///     </item>
    ///     <item>
    ///         <description>aes256-gcm@openssh.com</description>
    ///     </item>
    ///     <item>
    ///         <description>chacha20-poly1305@openssh.com</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public partial class PrivateKeyFile : IPrivateKeySource, IDisposable
    {
        private const string PrivateKeyPattern = @"^-+ *BEGIN (?<keyName>\w+( \w+)*) *-+\r?\n((Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\r?\n\r?\n)|(Comment: ""?[^\r\n]*""?\r?\n))?(?<data>([a-zA-Z0-9/+=]{1,80}\r?\n)+)(\r?\n)?-+ *END \k<keyName> *-+";

#if NET7_0_OR_GREATER
        private static readonly Regex PrivateKeyRegex = GetPrivateKeyRegex();

        [GeneratedRegex(PrivateKeyPattern, RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex GetPrivateKeyRegex();
#else
        private static readonly Regex PrivateKeyRegex = new Regex(PrivateKeyPattern,
                                                                  RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
#endif

        private readonly List<HostAlgorithm> _hostAlgorithms = new List<HostAlgorithm>();
        private Key _key;
        private bool _isDisposed;

        /// <summary>
        /// Gets the supported host algorithms for this key file.
        /// </summary>
        public IReadOnlyCollection<HostAlgorithm> HostKeyAlgorithms
        {
            get
            {
                return _hostAlgorithms;
            }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public Key Key
        {
            get
            {
                return _key;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public PrivateKeyFile(Key key)
        {
            _key = key;
            _hostAlgorithms.Add(new KeyHostAlgorithm(key.ToString(), key));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public PrivateKeyFile(Stream privateKey)
        {
            Open(privateKey, passPhrase: null);
            Debug.Assert(_hostAlgorithms.Count > 0, $"{nameof(HostKeyAlgorithms)} is not set.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/> or empty.</exception>
        /// <remarks>
        /// This method calls <see cref="File.Open(string, FileMode)"/> internally, this method does not catch exceptions from <see cref="File.Open(string, FileMode)"/>.
        /// </remarks>
        public PrivateKeyFile(string fileName)
            : this(fileName, passPhrase: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/> or empty, or <paramref name="passPhrase"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method calls <see cref="File.Open(string, FileMode)"/> internally, this method does not catch exceptions from <see cref="File.Open(string, FileMode)"/>.
        /// </remarks>
        public PrivateKeyFile(string fileName, string passPhrase)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            using (var keyFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Open(keyFile, passPhrase);
            }

            Debug.Assert(_hostAlgorithms.Count > 0, $"{nameof(HostKeyAlgorithms)} is not set.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="privateKey"/> or <paramref name="passPhrase"/> is <see langword="null"/>.</exception>
        public PrivateKeyFile(Stream privateKey, string passPhrase)
        {
            Open(privateKey, passPhrase);

            Debug.Assert(_hostAlgorithms.Count > 0, $"{nameof(HostKeyAlgorithms)} is not set.");
        }

        /// <summary>
        /// Opens the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        private void Open(Stream privateKey, string passPhrase)
        {
            if (privateKey is null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            Match privateKeyMatch;

            using (var sr = new StreamReader(privateKey))
            {
                var text = sr.ReadToEnd();
                privateKeyMatch = PrivateKeyRegex.Match(text);
            }

            if (!privateKeyMatch.Success)
            {
                throw new SshException("Invalid private key file.");
            }

            var keyName = privateKeyMatch.Result("${keyName}");
            if (!keyName.EndsWith("PRIVATE KEY", StringComparison.Ordinal))
            {
                throw new SshException("Invalid private key file.");
            }

            var cipherName = privateKeyMatch.Result("${cipherName}");
            var salt = privateKeyMatch.Result("${salt}");
            var data = privateKeyMatch.Result("${data}");

            var binaryData = Convert.FromBase64String(data);

            byte[] decryptedData;

            if (!string.IsNullOrEmpty(cipherName) && !string.IsNullOrEmpty(salt))
            {
                if (string.IsNullOrEmpty(passPhrase))
                {
                    throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                }

                var binarySalt = new byte[salt.Length / 2];
                for (var i = 0; i < binarySalt.Length; i++)
                {
                    binarySalt[i] = Convert.ToByte(salt.Substring(i * 2, 2), 16);
                }

                CipherInfo cipher;
                switch (cipherName)
                {
                    case "DES-EDE3-CBC":
                        cipher = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                        break;
                    case "DES-EDE3-CFB":
                        cipher = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CfbCipherMode(iv), new PKCS7Padding()));
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
                        throw new SshException(string.Format(CultureInfo.InvariantCulture, "Private key cipher \"{0}\" is not supported.", cipherName));
                }

                decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
            }
            else
            {
                decryptedData = binaryData;
            }

            switch (keyName)
            {
                case "RSA PRIVATE KEY":
                    _key = ParseRSAPrivateKey_Pkcs1(decryptedData);
                    break;
                case "DSA PRIVATE KEY":
                    _key = ParseDSSPrivateKey_OpenSSL(decryptedData);
                    break;
                case "EC PRIVATE KEY":
                    _key = ParseECPrivateKey_SEC1(decryptedData);
                    break;
                case "PRIVATE KEY":
                    var privateKeyInfo = PrivateKeyInfo.GetInstance(binaryData);
                    _key = ParsePkcs8PrivateKey(privateKeyInfo);
                    break;
                case "ENCRYPTED PRIVATE KEY":
                    var encryptedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(binaryData);
                    privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(passPhrase.ToCharArray(), encryptedPrivateKeyInfo);
                    _key = ParsePkcs8PrivateKey(privateKeyInfo);
                    break;
                case "OPENSSH PRIVATE KEY":
                    _key = ParseOpenSshV1Key(decryptedData, passPhrase);
                    break;
                case "SSH2 ENCRYPTED PRIVATE KEY":
                    var reader = new SshDataReader(decryptedData);
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
                        if (string.IsNullOrEmpty(passPhrase))
                        {
                            throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                        }

                        var key = GetCipherKey(passPhrase, 192 / 8);
                        var ssh2Сipher = new TripleDesCipher(key, new CbcCipherMode(new byte[8]), new PKCS7Padding());
                        keyData = ssh2Сipher.Decrypt(reader.ReadBytes(blobSize));
                    }
                    else
                    {
                        throw new SshException(string.Format("Cipher method '{0}' is not supported.", cipherName));
                    }

                    /*
                     * TODO: Create two specific data types to avoid using SshDataReader class.
                     */

                    reader = new SshDataReader(keyData);

                    var decryptedLength = reader.ReadUInt32();

                    if (decryptedLength > blobSize - 4)
                    {
                        throw new SshException("Invalid passphrase.");
                    }

                    if (keyType.Contains("rsa"))
                    {
                        var exponent = reader.ReadBigIntWithBits(); // e
                        var d = reader.ReadBigIntWithBits(); // d
                        var modulus = reader.ReadBigIntWithBits(); // n
                        var inverseQ = reader.ReadBigIntWithBits(); // u
                        var q = reader.ReadBigIntWithBits(); // p
                        var p = reader.ReadBigIntWithBits(); // q
                        _key = new RsaKey(modulus, exponent, d, p, q, inverseQ);
                    }
                    else if (keyType.Contains("dsa"))
                    {
                        var zero = reader.ReadUInt32();
                        if (zero != 0)
                        {
                            throw new SshException("Invalid private key");
                        }

                        var p = reader.ReadBigIntWithBits();
                        var g = reader.ReadBigIntWithBits();
                        var q = reader.ReadBigIntWithBits();
                        var y = reader.ReadBigIntWithBits();
                        var x = reader.ReadBigIntWithBits();
                        _key = new DsaKey(p, q, g, y, x);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Key type '{0}' is not supported.", keyType));
                    }

                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", keyName));
            }

            if (_key is RsaKey parsedRsaKey)
            {
                _hostAlgorithms.Add(new KeyHostAlgorithm("ssh-rsa", _key));
#pragma warning disable CA2000 // Dispose objects before losing scope
                _hostAlgorithms.Add(new KeyHostAlgorithm("rsa-sha2-512", _key, new RsaDigitalSignature(parsedRsaKey, HashAlgorithmName.SHA512)));
                _hostAlgorithms.Add(new KeyHostAlgorithm("rsa-sha2-256", _key, new RsaDigitalSignature(parsedRsaKey, HashAlgorithmName.SHA256)));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else
            {
                _hostAlgorithms.Add(new KeyHostAlgorithm(_key.ToString(), _key));
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
            if (cipherInfo is null)
            {
                throw new ArgumentNullException(nameof(cipherInfo));
            }

            if (cipherData is null)
            {
                throw new ArgumentNullException(nameof(cipherData));
            }

            if (binarySalt is null)
            {
                throw new ArgumentNullException(nameof(binarySalt));
            }

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

        /// <summary>
        /// Parses an OpenSSH V1 key file (i.e. ED25519 key) according to the the key spec:
        /// https://github.com/openssh/openssh-portable/blob/master/PROTOCOL.key.
        /// </summary>
        /// <param name="keyFileData">The key file data (i.e. base64 encoded data between the header/footer).</param>
        /// <param name="passPhrase">Passphrase or <see langword="null"/> if there isn't one.</param>
        /// <returns>
        /// The OpenSSH V1 key.
        /// </returns>
        private static Key ParseOpenSshV1Key(byte[] keyFileData, string passPhrase)
        {
            var keyReader = new SshDataReader(keyFileData);

            // check magic header
            var authMagic = Encoding.UTF8.GetBytes("openssh-key-v1\0");
            var keyHeaderBytes = keyReader.ReadBytes(authMagic.Length);
            if (!authMagic.IsEqualTo(keyHeaderBytes))
            {
                throw new SshException("This openssh key does not contain the 'openssh-key-v1' format magic header");
            }

            // cipher will be "aes256-cbc" or other cipher if using a passphrase, "none" otherwise
            var cipherName = keyReader.ReadString(Encoding.UTF8);

            // key derivation function (kdf): bcrypt or nothing
            var kdfName = keyReader.ReadString(Encoding.UTF8);

            // kdf options length: 24 if passphrase, 0 if no passphrase
            var kdfOptionsLen = (int)keyReader.ReadUInt32();
            byte[] salt = null;
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

            // read public key in ssh-format, but we dont need it
            _ = keyReader.ReadString(Encoding.UTF8);

            // possibly encrypted private key
            var privateKeyLength = (int)keyReader.ReadUInt32();
            byte[] privateKeyBytes;

            // decrypt private key if necessary
            if (cipherName != "none")
            {
                if (string.IsNullOrEmpty(passPhrase))
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
                        cipherInfo = new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CbcCipherMode(iv), padding: null));
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
                var passPhraseBytes = Encoding.UTF8.GetBytes(passPhrase);
                var keyiv = new byte[keyLength + ivLength];
                new BCrypt().Pbkdf(passPhraseBytes, salt, rounds, keyiv);

                var key = keyiv.Take(keyLength);
                var iv = keyiv.Take(keyLength, ivLength);

                var cipher = cipherInfo.Cipher(key, iv);
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
            var privateKeyReader = new SshDataReader(privateKeyBytes);

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
                    _ = privateKeyReader.ReadBignum2();

                    // k || ENC(A)
                    unencryptedPrivateKey = privateKeyReader.ReadBignum2();
                    parsedKey = new ED25519Key(unencryptedPrivateKey);
                    break;
                case "ecdsa-sha2-nistp256":
                case "ecdsa-sha2-nistp384":
                case "ecdsa-sha2-nistp521":
                    // curve
                    var len = (int)privateKeyReader.ReadUInt32();
                    var curveName = Encoding.ASCII.GetString(privateKeyReader.ReadBytes(len));
                    var curveOid = SshNamedCurves.GetOid(curveName).GetID();

                    // public key
                    publicKey = privateKeyReader.ReadBignum2();

                    // private key
                    unencryptedPrivateKey = privateKeyReader.ReadBignum2();
                    parsedKey = new EcdsaKey(curveOid, publicKey, unencryptedPrivateKey);
                    break;
                case "ssh-rsa":
                    var modulus = privateKeyReader.ReadBignum(); // n
                    var exponent = privateKeyReader.ReadBignum(); // e
                    var d = privateKeyReader.ReadBignum(); // d
                    var inverseQ = privateKeyReader.ReadBignum(); // iqmp
                    var p = privateKeyReader.ReadBignum(); // p
                    var q = privateKeyReader.ReadBignum(); // q
                    parsedKey = new RsaKey(modulus, exponent, d, p, q, inverseQ);
                    break;
                default:
                    throw new SshException("OpenSSH key type '" + keyType + "' is not supported.");
            }

            parsedKey.Comment = privateKeyReader.ReadString(Encoding.UTF8);

            // The list of privatekey/comment pairs is padded with the bytes 1, 2, 3, ...
            // until the total length is a multiple of the cipher block size.
            var padding = privateKeyReader.ReadBytes();
            for (var i = 0; i < padding.Length; i++)
            {
                if ((int)padding[i] != i + 1)
                {
                    throw new SshException("Padding of openssh key format contained wrong byte at position: " +
                                           i.ToString(CultureInfo.InvariantCulture));
                }
            }

            return parsedKey;
        }

        /// <summary>
        /// Parses PKCS#8 PrivateKeyInfo.
        /// </summary>
        /// <param name="privateKeyInfo">The PKCS#8 PrivateKeyInfo.</param>
        /// <remarks>
        /// <see href="https://www.rfc-editor.org/rfc/rfc5208#section-5" />.
        /// </remarks>
        /// <returns>The <see cref="Key" />.</returns>
        /// <exception cref="SshException">Algorithm not supported.</exception>
        private static Key ParsePkcs8PrivateKey(PrivateKeyInfo privateKeyInfo)
        {
            var algorithmOid = privateKeyInfo.PrivateKeyAlgorithm.Algorithm;
            var key = privateKeyInfo.PrivateKey.GetOctets();
            if (algorithmOid.Equals(PkcsObjectIdentifiers.RsaEncryption))
            {
                return ParseRSAPrivateKey_Pkcs1(key);
            }

            if (algorithmOid.Equals(X9ObjectIdentifiers.IdDsa))
            {
                var parameters = privateKeyInfo.PrivateKeyAlgorithm.Parameters.GetDerEncoded();
                return ParseDSAPrivateKey_Pkcs8_OpenSSL(parameters, key);
            }

            if (algorithmOid.Equals(X9ObjectIdentifiers.IdECPublicKey))
            {
                var parameters2 = privateKeyInfo.PrivateKeyAlgorithm.Parameters.GetDerEncoded();
                return ParseECPrivateKey_Pkcs8_OpenSSL(parameters2, key);
            }

            if (algorithmOid.Equals(EdECObjectIdentifiers.id_Ed25519))
            {
                return new ED25519Key(key);
            }

            throw new SshException(string.Format(CultureInfo.InvariantCulture, "Private key algorithm \"{0}\" is not supported.", algorithmOid));
        }

        /// <summary>
        /// Parses PKCS#1 RSA private key.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.rfc-editor.org/rfc/rfc2437#section-11.1.2" />.
        /// <code>
        /// RSAPrivateKey ::= SEQUENCE {
        ///     version INTEGER,
        ///     modulus INTEGER, -- n
        ///     publicExponent INTEGER, -- e
        ///     privateExponent INTEGER, -- d
        ///     prime1 INTEGER, -- p
        ///     prime2 INTEGER, -- q
        ///     exponent1 INTEGER, -- d mod (p-1)
        ///     exponent2 INTEGER, -- d mod (q-1)
        ///     coefficient INTEGER -- (inverse of q) mod p
        /// }
        /// </code>
        /// </remarks>
        /// <param name="keyData">The key data.</param>
        /// <returns>The <see cref="RsaKey" />.</returns>
        private static RsaKey ParseRSAPrivateKey_Pkcs1(byte[] keyData)
        {
            var keyReader = new AsnReader(keyData, AsnEncodingRules.BER);
            var sequenceReader = keyReader.ReadSequence();

            // Some key has extra byte, for example 'Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt' at the test data folder.
            ////keyReader.ThrowIfNotEmpty();

            var version = sequenceReader.ReadInteger();
            if (version != BigInteger.Zero)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "RSA version '{0}' is not supported.", version));
            }

            var modulus = sequenceReader.ReadInteger();
            var exponent = sequenceReader.ReadInteger();
            var d = sequenceReader.ReadInteger();
            var p = sequenceReader.ReadInteger();
            var q = sequenceReader.ReadInteger();
            var dp = sequenceReader.ReadInteger();
            var dq = sequenceReader.ReadInteger();
            var inverseQ = sequenceReader.ReadInteger();
            sequenceReader.ThrowIfNotEmpty();

            return new RsaKey(modulus, exponent, d, p, q, dp, dq, inverseQ);
        }

        /// <summary>
        /// Parses OpenSSL DSA private key.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <remarks>
        /// OpenSSL produces ASN.1 DER encoded form of an ASN.1 SEQUENCE consisting of the values of
        /// version (currently zero), p, q, g, the public and private key components respectively as ASN.1 INTEGERs.
        /// <see href="https://docs.openssl.org/1.1.1/man1/dsa/#options" />.
        /// <code>
        /// DSSPrivatKey_OpenSSL ::= SEQUENCE {
        ///     version INTEGER,
        ///     p INTEGER,
        ///     q INTEGER,
        ///     g INTEGER,
        ///     y INTEGER,
        ///     x INTEGER
        /// }
        /// </code>
        /// </remarks>
        /// <returns>The <see cref="DsaKey" />.</returns>
        private static DsaKey ParseDSSPrivateKey_OpenSSL(byte[] keyData)
        {
            var keyReader = new AsnReader(keyData, AsnEncodingRules.BER);
            var sequenceReader = keyReader.ReadSequence();
            keyReader.ThrowIfNotEmpty();

            var version = sequenceReader.ReadInteger();
            if (version != BigInteger.Zero)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "DSA version '{0}' is not supported.", version));
            }

            var p = sequenceReader.ReadInteger();
            var q = sequenceReader.ReadInteger();
            var g = sequenceReader.ReadInteger();
            var y = sequenceReader.ReadInteger();
            var x = sequenceReader.ReadInteger();
            sequenceReader.ThrowIfNotEmpty();

            return new DsaKey(p, q, g, y, x);
        }

        /// <summary>
        /// Parses OpenSSL DSA private key in Pkcs#8 format.
        /// </summary>
        /// <param name="parametersData">The parameters data.</param>
        /// <param name="keyData">The key data.</param>
        /// <remarks>
        /// The format of PKCS#8 DSA (and other) private keys is not well documented:
        /// it is hidden away in PKCS#11 v2.01, section 11.9.
        /// OpenSSL's default DSA PKCS#8 private key format complies with this standard.
        /// <see href="https://docs.openssl.org/1.1.1/man1/pkcs8/#standards" />
        /// <see href="https://www.cryptsoft.com/pkcs11doc/STANDARD/v201-95.pdf" />.
        /// <code>
        /// Dss-Parms ::= SEQUENCE {
        ///     p INTEGER,
        ///     q INTEGER,
        ///     g INTEGER
        /// }
        /// </code>
        /// DSAprivatekeys are represented as BER-encoded ASN.1 type INTEGER.
        /// </remarks>
        /// <returns>The <see cref="DsaKey" />.</returns>
        private static DsaKey ParseDSAPrivateKey_Pkcs8_OpenSSL(byte[] parametersData, byte[] keyData)
        {
            var parametersReader = new AsnReader(parametersData, AsnEncodingRules.BER);
            var sequenceReader = parametersReader.ReadSequence();
            parametersReader.ThrowIfNotEmpty();

            var p = sequenceReader.ReadInteger();
            var q = sequenceReader.ReadInteger();
            var g = sequenceReader.ReadInteger();
            sequenceReader.ThrowIfNotEmpty();

            var keyReader = new AsnReader(keyData, AsnEncodingRules.BER);
            var x = keyReader.ReadInteger();
            keyReader.ThrowIfNotEmpty();

            var y = BigInteger.ModPow(g, x, p);

            return new DsaKey(p, q, g, y, x);
        }

        /// <summary>
        /// Parses OpenSSL EC private key.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <remarks>
        /// OpenSSL produces ASN.1 DER encoded SEC1 private key.
        /// <see href="https://docs.openssl.org/1.1.1/man1/ec/#description" />
        /// <see href="https://www.secg.org/sec1-v2.pdf" />.
        /// Republished as RFC5915 <see href="https://www.rfc-editor.org/rfc/rfc5915" />,
        /// See answer at <see href="https://crypto.stackexchange.com/a/48709" />.
        /// <code>
        /// ECPrivateKey ::= SEQUENCE {
        ///     version INTEGER { ecPrivkeyVer1(1) } (ecPrivkeyVer1),
        ///     privateKey OCTET STRING,
        ///     parameters [0] ECDomainParameters {{ SECGCurveNames }} OPTIONAL,
        ///     publicKey [1] BIT STRING OPTIONAL
        /// }
        /// </code>
        /// When generating a transfer encoding, generators SHOULD use
        /// Distinguished Encoding Rules (DER) and receivers SHOULD be
        /// prepared to handle Basic Encoding Rules (BER) and DER.
        /// </remarks>
        /// <returns>The <see cref="EcdsaKey" />.</returns>
        private static EcdsaKey ParseECPrivateKey_SEC1(byte[] keyData)
        {
            var keyReader = new AsnReader(keyData, AsnEncodingRules.DER);
            var sequenceReader = keyReader.ReadSequence();
            keyReader.ThrowIfNotEmpty();

            var version = sequenceReader.ReadInteger();
            if (version != BigInteger.One)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "EC version '{0}' is not supported.", version));
            }

            var privatekey = sequenceReader.ReadOctetString();

            // Though the ASN.1 indicates that the parameters field is OPTIONAL,
            // implementations that conform to this document MUST always include the parameters field.
            var parametersReader = sequenceReader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true));
            var curve = parametersReader.ReadObjectIdentifier();
            parametersReader.ThrowIfNotEmpty();

            // Though the ASN.1 indicates publicKey is OPTIONAL,
            // implementations that conform to this document SHOULD always include the publicKey field.
            var publicKeyReader = sequenceReader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true));
            var publickey = publicKeyReader.ReadBitString(out _);
            publicKeyReader.ThrowIfNotEmpty();

            sequenceReader.ThrowIfNotEmpty();

            return new EcdsaKey(curve, publickey, privatekey);
        }

        /// <summary>
        /// Parses OpenSSL EC private key in Pkcs#8 format.
        /// </summary>
        /// <param name="parametersData">The parameters data.</param>
        /// <param name="keyData">The key data.</param>
        /// <remarks>
        /// The format of PKCS#8 EC private keys is not well documented.
        /// </remarks>
        /// <returns>The <see cref="EcdsaKey" />.</returns>
        private static EcdsaKey ParseECPrivateKey_Pkcs8_OpenSSL(byte[] parametersData, byte[] keyData)
        {
            var parametersReader = new AsnReader(parametersData, AsnEncodingRules.DER);
            var curve = parametersReader.ReadObjectIdentifier();
            parametersReader.ThrowIfNotEmpty();

            var privateKeyReader = new AsnReader(keyData, AsnEncodingRules.DER);
            var sequenceReader = privateKeyReader.ReadSequence();
            privateKeyReader.ThrowIfNotEmpty();

            var version = sequenceReader.ReadInteger();
            if (version != BigInteger.One)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "EC version '{0}' is not supported.", version));
            }

            var privatekey = sequenceReader.ReadOctetString();

            var publicKeyReader = sequenceReader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true));
            var publickey = publicKeyReader.ReadBitString(out _);
            publicKeyReader.ThrowIfNotEmpty();

            sequenceReader.ThrowIfNotEmpty();

            return new EcdsaKey(curve, publickey, privatekey);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var key = _key;
                if (key != null)
                {
                    ((IDisposable)key).Dispose();
                    _key = null;
                }

                _isDisposed = true;
            }
        }

        private sealed class SshDataReader : SshData
        {
            public SshDataReader(byte[] data)
            {
                Load(data);
            }

            public new uint ReadUInt32()
            {
                return base.ReadUInt32();
            }

            public new string ReadString(Encoding encoding)
            {
                return base.ReadString(encoding);
            }

            public new byte[] ReadBytes(int length)
            {
                return base.ReadBytes(length);
            }

            public new byte[] ReadBytes()
            {
                return base.ReadBytes();
            }

            /// <summary>
            /// Reads next mpint data type from internal buffer where length specified in bits.
            /// </summary>
            /// <returns>mpint read.</returns>
            public BigInteger ReadBigIntWithBits()
            {
                var length = (int)base.ReadUInt32();

                length = (length + 7) / 8;

                return base.ReadBytes(length).ToBigInteger2();
            }

            public BigInteger ReadBignum()
            {
                return DataStream.ReadBigInt();
            }

            public byte[] ReadBignum2()
            {
                return ReadBinary();
            }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
            }
        }
    }
}
