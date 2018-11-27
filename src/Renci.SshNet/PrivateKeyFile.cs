using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Security;
using Renci.SshNet.Common;
using System.Globalization;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using System.Diagnostics.CodeAnalysis;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key information.
    /// </summary>
    /// <example>
    ///     <code source="..\..\src\Renci.SshNet.Tests\Data\Key.RSA.txt" language="Text" title="Private RSA key example" />
    /// </example>
    /// <remarks>
    /// <para>
    /// The following private keys are supported:
    /// <list type="bullet">
    ///     <item>
    ///         <description>RSA in OpenSSL PEM and ssh.com format</description>
    ///     </item>
    ///     <item>
    ///         <description>DSA in OpenSSL PEM and ssh.com format</description>
    ///     </item>
    ///     <item>
    ///         <description>ECDSA 256/384/521 in OpenSSL PEM format</description>
    ///     </item>
    ///     <item>
    ///         <description>ED25519 in OpenSSH key format</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The following encryption algorithms are supported:
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
    /// </remarks>
    public class PrivateKeyFile : IDisposable
    {
        private static readonly Regex PrivateKeyRegex = new Regex(@"^-+ *BEGIN (?<keyName>\w+( \w+)*) PRIVATE KEY *-+\r?\n((Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\r?\n\r?\n)|(Comment: ""?[^\r\n]*""?\r?\n))?(?<data>([a-zA-Z0-9/+=]{1,80}\r?\n)+)-+ *END \k<keyName> PRIVATE KEY *-+",
#if FEATURE_REGEX_COMPILE
            RegexOptions.Compiled | RegexOptions.Multiline);
#else
            RegexOptions.Multiline);
#endif

        private Key _key;

        /// <summary>
        /// Gets the host key.
        /// </summary>
        public HostAlgorithm HostKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public PrivateKeyFile(Stream privateKey)
        {
            Open(privateKey, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c> or empty.</exception>
        /// <remarks>This method calls <see cref="System.IO.File.Open(string, System.IO.FileMode)"/> internally, this method does not catch exceptions from <see cref="System.IO.File.Open(string, System.IO.FileMode)"/>.</remarks>
        public PrivateKeyFile(string fileName)
            : this(fileName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c> or empty, or <paramref name="passPhrase"/> is <c>null</c>.</exception>
        /// <remarks>This method calls <see cref="System.IO.File.Open(string, System.IO.FileMode)"/> internally, this method does not catch exceptions from <see cref="System.IO.File.Open(string, System.IO.FileMode)"/>.</remarks>
        public PrivateKeyFile(string fileName, string passPhrase)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            using (var keyFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Open(keyFile, passPhrase);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="privateKey"/> or <paramref name="passPhrase"/> is <c>null</c>.</exception>
        public PrivateKeyFile(Stream privateKey, string passPhrase)
        {
            Open(privateKey, passPhrase);
        }

        /// <summary>
        /// Opens the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "this._key disposed in Dispose(bool) method.")]
        private void Open(Stream privateKey, string passPhrase)
        {
            if (privateKey == null)
                throw new ArgumentNullException("privateKey");

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
            var cipherName = privateKeyMatch.Result("${cipherName}");
            var salt = privateKeyMatch.Result("${salt}");
            var data = privateKeyMatch.Result("${data}");

            var binaryData = Convert.FromBase64String(data);

            byte[] decryptedData;

            if (!string.IsNullOrEmpty(cipherName) && !string.IsNullOrEmpty(salt))
            {
                if (string.IsNullOrEmpty(passPhrase))
                    throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");

                var binarySalt = new byte[salt.Length / 2];
                for (var i = 0; i < binarySalt.Length; i++)
                    binarySalt[i] = Convert.ToByte(salt.Substring(i * 2, 2), 16);

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
                        cipher = new CipherInfo(128, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                        break;
                    case "AES-192-CBC":
                        cipher = new CipherInfo(192, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                        break;
                    case "AES-256-CBC":
                        cipher = new CipherInfo(256, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()));
                        break;
                    default:
                        throw new SshException(string.Format(CultureInfo.CurrentCulture, "Private key cipher \"{0}\" is not supported.", cipherName));
                }

                decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
            }
            else
            {
                decryptedData = binaryData;
            }

            switch (keyName)
            {
                case "RSA":
                    _key = new RsaKey(decryptedData);
                    HostKey = new KeyHostAlgorithm("ssh-rsa", _key);
                    break;
                case "DSA":
                    _key = new DsaKey(decryptedData);
                    HostKey = new KeyHostAlgorithm("ssh-dss", _key);
                    break;
#if FEATURE_ECDSA
                case "EC":
                    _key = new EcdsaKey(decryptedData);
                    HostKey = new KeyHostAlgorithm(_key.ToString(), _key);
                    break;
#endif
                case "OPENSSH":
                    _key = ParseOpenSshV1Key(decryptedData, passPhrase);
                    HostKey = new KeyHostAlgorithm(_key.ToString(), _key);
                    break;
                case "SSH2 ENCRYPTED":
                    var reader = new SshDataReader(decryptedData);
                    var magicNumber = reader.ReadUInt32();
                    if (magicNumber != 0x3f6ff9eb)
                    {
                        throw new SshException("Invalid SSH2 private key.");
                    }

                    reader.ReadUInt32(); //  Read total bytes length including magic number
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
                            throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");

                        var key = GetCipherKey(passPhrase, 192 / 8);
                        var ssh2Сipher = new TripleDesCipher(key, new CbcCipherMode(new byte[8]), new PKCS7Padding());
                        keyData = ssh2Сipher.Decrypt(reader.ReadBytes(blobSize));
                    }
                    else
                    {
                        throw new SshException(string.Format("Cipher method '{0}' is not supported.", cipherName));
                    }

                    //  TODO:   Create two specific data types to avoid using SshDataReader class

                    reader = new SshDataReader(keyData);

                    var decryptedLength = reader.ReadUInt32();

                    if (decryptedLength > blobSize - 4)
                        throw new SshException("Invalid passphrase.");
                    
                    if (keyType == "if-modn{sign{rsa-pkcs1-sha1},encrypt{rsa-pkcs1v2-oaep}}")
                    {
                        var exponent = reader.ReadBigIntWithBits();//e
                        var d = reader.ReadBigIntWithBits();//d
                        var modulus = reader.ReadBigIntWithBits();//n
                        var inverseQ = reader.ReadBigIntWithBits();//u
                        var q = reader.ReadBigIntWithBits();//p
                        var p = reader.ReadBigIntWithBits();//q
                        _key = new RsaKey(modulus, exponent, d, p, q, inverseQ);
                        HostKey = new KeyHostAlgorithm("ssh-rsa", _key);
                    }
                    else if (keyType == "dl-modp{sign{dsa-nist-sha1},dh{plain}}")
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
                        HostKey = new KeyHostAlgorithm("ssh-dss", _key);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Key type '{0}' is not supported.", keyType));
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", keyName));
            }
        }

        private static byte[] GetCipherKey(string passphrase, int length)
        {
            var cipherKey = new List<byte>();

            using (var md5 = CryptoAbstraction.CreateMD5())
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
        /// <exception cref="ArgumentNullException"><paramref name="cipherInfo" />, <paramref name="cipherData" />, <paramref name="passPhrase" /> or <paramref name="binarySalt" /> is <c>null</c>.</exception>
        private static byte[] DecryptKey(CipherInfo cipherInfo, byte[] cipherData, string passPhrase, byte[] binarySalt)
        {
            if (cipherInfo == null)
                throw new ArgumentNullException("cipherInfo");

            if (cipherData == null)
                throw new ArgumentNullException("cipherData");

            if (binarySalt == null)
                throw new ArgumentNullException("binarySalt");

            var cipherKey = new List<byte>();

            using (var md5 = CryptoAbstraction.CreateMD5())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(passPhrase);

                //  Use 8 bytes binary salt
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

            var cipher = cipherInfo.Cipher(cipherKey.ToArray(), binarySalt);

            return cipher.Decrypt(cipherData);
        }

        /// <summary>
        /// Parses an OpenSSH V1 key file (i.e. ED25519 key) according to the the key spec:
        /// https://github.com/openssh/openssh-portable/blob/master/PROTOCOL.key.
        /// </summary>
        /// <param name="keyFileData">the key file data (i.e. base64 encoded data between the header/footer)</param>
        /// <param name="passPhrase">passphrase or null if there isn't one</param>
        /// <returns></returns>
        private ED25519Key ParseOpenSshV1Key(byte [] keyFileData, string passPhrase)
        {
            var keyReader = new SshDataReader(keyFileData);

            //check magic header
            var authMagic = Encoding.UTF8.GetBytes("openssh-key-v1\0");
            var keyHeaderBytes = keyReader.ReadBytes(authMagic.Length);
            if (!authMagic.IsEqualTo(keyHeaderBytes))
            {
                throw new SshException("This openssh key does not contain the 'openssh-key-v1' format magic header");
            }

            //cipher will be "aes256-cbc" if using a passphrase, "none" otherwise
            var cipherName = keyReader.ReadString(Encoding.UTF8);
            //key derivation function (kdf): bcrypt or nothing
            var kdfName = keyReader.ReadString(Encoding.UTF8);
            //kdf options length: 24 if passphrase, 0 if no passphrase
            var kdfOptionsLen = (int)keyReader.ReadUInt32();
            byte[] salt = null;
            int rounds = 0;
            if (kdfOptionsLen > 0)
            {
                var saltLength = (int)keyReader.ReadUInt32();
                salt = keyReader.ReadBytes(saltLength);
                rounds = (int)keyReader.ReadUInt32();
            }

            //number of public keys, only supporting 1 for now
            var numberOfPublicKeys = (int)keyReader.ReadUInt32();
            if (numberOfPublicKeys != 1)
            {
                throw new SshException("At this time only one public key in the openssh key is supported.");
            }

            //length of first public key section
            keyReader.ReadUInt32();
            var keyType = keyReader.ReadString(Encoding.UTF8);
            if(keyType != "ssh-ed25519")
            {
                throw new SshException("openssh key type: " + keyType + " is not supported");
            }

            //read public key
            var publicKeyLength = (int)keyReader.ReadUInt32(); //32
            var publicKey = keyReader.ReadBytes(publicKeyLength);

            //possibly encrypted private key
            var privateKeyLength = (int)keyReader.ReadUInt32();
            var privateKeyBytes = keyReader.ReadBytes(privateKeyLength);

            //decrypt private key if necessary
            if (cipherName == "aes256-cbc")
            {
                if (string.IsNullOrEmpty(passPhrase))
                {
                    throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
                }
                if (string.IsNullOrEmpty(kdfName) || kdfName != "bcrypt")
                {
                    throw new SshException("kdf " + kdfName + " is not supported for openssh key file");
                }

                //inspired by the SSHj library (https://github.com/hierynomus/sshj)
                //apply the kdf to derive a key and iv from the passphrase
                var passPhraseBytes = Encoding.UTF8.GetBytes(passPhrase);
                byte[] keyiv = new byte[48];
                new BCrypt().Pbkdf(passPhraseBytes, salt, rounds, keyiv);
                byte[] key = new byte[32];
                Array.Copy(keyiv, 0, key, 0, 32);
                byte[] iv = new byte[16];
                Array.Copy(keyiv, 32, iv, 0, 16);

                //now that we have the key/iv, use a cipher to decrypt the bytes
                var cipher = new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding());
                privateKeyBytes = cipher.Decrypt(privateKeyBytes);
            }
            else if (cipherName != "none")
            {
                throw new SshException("cipher name " + cipherName + " for openssh key file is not supported");
            }

            //validate private key length
            privateKeyLength = privateKeyBytes.Length;
            if (privateKeyLength % 8 != 0)
            {
                throw new SshException("The private key section must be a multiple of the block size (8)");
            }

            //now parse the data we called the private key, it actually contains the public key again
            //so we need to parse through it to get the private key bytes, plus there's some
            //validation we need to do.
            var privateKeyReader = new SshDataReader(privateKeyBytes);

            //check ints should match, they wouldn't match for example if the wrong passphrase was supplied
            int checkInt1 = (int)privateKeyReader.ReadUInt32();
            int checkInt2 = (int)privateKeyReader.ReadUInt32();
            if (checkInt1 != checkInt2)
            {
                throw new SshException("The checkints differed, the openssh key was not correctly decoded.");
            }

            //key type, we already know it is ssh-ed25519
            privateKeyReader.ReadString(Encoding.UTF8);

            //public key length/bytes (again)
            var publicKeyLength2 = (int)privateKeyReader.ReadUInt32();
            privateKeyReader.ReadBytes(publicKeyLength2);

            //length of private and public key (64)
            privateKeyReader.ReadUInt32();
            var unencryptedPrivateKey = privateKeyReader.ReadBytes(32);
            //public key (again)
            privateKeyReader.ReadBytes(32);

            //comment, we don't need this but we could log it, not sure if necessary
            var comment = privateKeyReader.ReadString(Encoding.UTF8);

            //The list of privatekey/comment pairs is padded with the bytes 1, 2, 3, ...
            //until the total length is a multiple of the cipher block size.
            var padding = privateKeyReader.ReadBytes();
            for (int i = 0; i < padding.Length; i++)
            {
                if ((int)padding[i] != i + 1)
                {
                    throw new SshException("Padding of openssh key format contained wrong byte at position: " + i);
                }
            }

            return new ED25519Key(publicKey.Reverse(), unencryptedPrivateKey);
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                var key = _key;
                if (key != null)
                {
                    ((IDisposable) key).Dispose();
                    _key = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PrivateKeyFile"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateKeyFile()
        {
            Dispose(false);
        }

        #endregion

        private class SshDataReader : SshData
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
                var length = (int) base.ReadUInt32();

                length = (length + 7) / 8;

                var data = base.ReadBytes(length);
                var bytesArray = new byte[data.Length + 1];
                Buffer.BlockCopy(data, 0, bytesArray, 1, data.Length);

                return new BigInteger(bytesArray.Reverse());
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
