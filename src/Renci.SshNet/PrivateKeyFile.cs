#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;

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
    ///         <description>RSA in OpenSSL PEM, ssh.com, OpenSSH and PuTTY key format</description>
    ///     </item>
    ///     <item>
    ///         <description>ECDSA 256/384/521 in OpenSSL PEM, OpenSSH and PuTTY key format</description>
    ///     </item>
    ///     <item>
    ///         <description>ED25519 in OpenSSL PEM, OpenSSH and PuTTY key format</description>
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
    /// The following encryption algorithms are supported for OpenSSH key format:
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
    /// <para>
    /// The following encryption algorithms are supported for PuTTY key format:
    /// <list type="bullet">
    ///     <item>
    ///         <description>aes256-cbc</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public partial class PrivateKeyFile : IPrivateKeySource, IDisposable
    {
        private const string PrivateKeyPattern = @"^-+ *BEGIN (?<keyName>\w+( \w+)*) *-+\r?\n((Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[a-fA-F0-9]+)\r?\n\r?\n)|(Comment: ""?[^\r\n]*""?\r?\n))?(?<data>([a-zA-Z0-9/+=]{1,80}\r?\n)+)(\r?\n)?-+ *END \k<keyName> *-+";
        private const string PuTTYPrivateKeyPattern = @"^(?<keyName>PuTTY-User-Key-File)-(?<version>\d+): (?<algorithmName>[\w-]+)\r?\nEncryption: (?<encryptionType>[\w-]+)\r?\nComment: (?<comment>.*?)\r?\nPublic-Lines: \d+\r?\n(?<publicKey>(([a-zA-Z0-9/+=]{1,64})\r?\n)+)(Key-Derivation: (?<argon2Type>\w+)\r?\nArgon2-Memory: (?<argon2Memory>\d+)\r?\nArgon2-Passes: (?<argon2Passes>\d+)\r?\nArgon2-Parallelism: (?<argon2Parallelism>\d+)\r?\nArgon2-Salt: (?<argon2Salt>[a-fA-F0-9]+)\r?\n)?Private-Lines: \d+\r?\n(?<data>(([a-zA-Z0-9/+=]{1,64})\r?\n)+)+Private-MAC: (?<mac>[a-fA-F0-9]+)";
        private const string CertificatePattern = @"(?<type>[-\w]+@openssh\.com)\s(?<data>[a-zA-Z0-9\/+=]*)(\s+(?<comment>.*))?";

#if NET
        private static readonly Regex PrivateKeyRegex = GetPrivateKeyRegex();
        private static readonly Regex PuTTYPrivateKeyRegex = GetPrivateKeyPuTTYRegex();
        private static readonly Regex CertificateRegex = GetCertificateRegex();

        [GeneratedRegex(PrivateKeyPattern, RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex GetPrivateKeyRegex();

        [GeneratedRegex(PuTTYPrivateKeyPattern, RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex GetPrivateKeyPuTTYRegex();

        [GeneratedRegex(CertificatePattern, RegexOptions.ExplicitCapture)]
        private static partial Regex GetCertificateRegex();
#else
        private static readonly Regex PrivateKeyRegex = new Regex(PrivateKeyPattern, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        private static readonly Regex PuTTYPrivateKeyRegex = new Regex(PuTTYPrivateKeyPattern, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        private static readonly Regex CertificateRegex = new Regex(CertificatePattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
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
        /// Gets the public key certificate associated with this key,
        /// or <see langword="null"/> if no certificate data
        /// has been passed to the constructor.
        /// </summary>
        public Certificate? Certificate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public PrivateKeyFile(Key key)
        {
            ThrowHelper.ThrowIfNull(key);

            _key = key;
            _hostAlgorithms.Add(new KeyHostAlgorithm(key.ToString(), key));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public PrivateKeyFile(Stream privateKey)
            : this(privateKey, passPhrase: null, certificate: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">The path of the private key file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method calls <see cref="File.Open(string, FileMode)"/> internally, this method does not catch exceptions from <see cref="File.Open(string, FileMode)"/>.
        /// </remarks>
        public PrivateKeyFile(string fileName)
            : this(fileName, passPhrase: null, certificateFileName: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">The path of the private key file.</param>
        /// <param name="passPhrase">The pass phrase for the private key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method calls <see cref="File.Open(string, FileMode)"/> internally, this method does not catch exceptions from <see cref="File.Open(string, FileMode)"/>.
        /// </remarks>
        public PrivateKeyFile(string fileName, string? passPhrase)
            : this(fileName, passPhrase, certificateFileName: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">The path of the private key file.</param>
        /// <param name="passPhrase">The pass phrase for the private key.</param>
        /// <param name="certificateFileName">The path of a certificate file which certifies the private key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        public PrivateKeyFile(string fileName, string? passPhrase, string? certificateFileName)
        {
            ThrowHelper.ThrowIfNull(fileName);

            using (var keyFile = File.OpenRead(fileName))
            {
                Open(keyFile, passPhrase);
            }

            if (certificateFileName is not null)
            {
                using (var certificateFile = File.OpenRead(certificateFileName))
                {
                    OpenCertificate(certificateFile);
                }

                Debug.Assert(Certificate is not null, $"{nameof(Certificate)} is null.");
            }

            Debug.Assert(Key is not null, $"{nameof(Key)} is null.");
            Debug.Assert(HostKeyAlgorithms.Count > 0, $"{nameof(HostKeyAlgorithms)} is not set.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="privateKey"/> is <see langword="null"/>.</exception>
        public PrivateKeyFile(Stream privateKey, string? passPhrase)
            : this(privateKey, passPhrase, certificate: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase for the private key.</param>
        /// <param name="certificate">A certificate which certifies the private key.</param>
        public PrivateKeyFile(Stream privateKey, string? passPhrase, Stream? certificate)
        {
            ThrowHelper.ThrowIfNull(privateKey);

            Open(privateKey, passPhrase);

            if (certificate is not null)
            {
                OpenCertificate(certificate);

                Debug.Assert(Certificate is not null, $"{nameof(Certificate)} is null.");
            }

            Debug.Assert(Key is not null, $"{nameof(Key)} is null.");
            Debug.Assert(HostKeyAlgorithms.Count > 0, $"{nameof(HostKeyAlgorithms)} is not set.");
        }

        /// <summary>
        /// Opens the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        [MemberNotNull(nameof(_key))]
        private void Open(Stream privateKey, string? passPhrase)
        {
            Debug.Assert(privateKey is not null, "Should have validated not-null in the constructor.");

            Match privateKeyMatch;

            using (var sr = new StreamReader(privateKey))
            {
                var text = sr.ReadToEnd();
                if (text.StartsWith("PuTTY-User-Key-File", StringComparison.Ordinal))
                {
                    privateKeyMatch = PuTTYPrivateKeyRegex.Match(text);
                }
                else
                {
                    privateKeyMatch = PrivateKeyRegex.Match(text);
                }
            }

            if (!privateKeyMatch.Success)
            {
                throw new SshException("Invalid private key file.");
            }

            var keyName = privateKeyMatch.Result("${keyName}");
            var data = privateKeyMatch.Result("${data}");
            var binaryData = Convert.FromBase64String(data);

            IPrivateKeyParser parser;
            switch (keyName)
            {
                case "RSA PRIVATE KEY":
                case "EC PRIVATE KEY":
                    var cipherName = privateKeyMatch.Result("${cipherName}");
                    var salt = privateKeyMatch.Result("${salt}");
                    parser = new PKCS1(cipherName, salt, keyName, binaryData, passPhrase);
                    break;
                case "PRIVATE KEY":
                    parser = new PKCS8(encrypted: false, binaryData, passPhrase);
                    break;
                case "ENCRYPTED PRIVATE KEY":
                    parser = new PKCS8(encrypted: true, binaryData, passPhrase);
                    break;
                case "OPENSSH PRIVATE KEY":
                    parser = new OpenSSH(binaryData, passPhrase);
                    break;
                case "SSH2 ENCRYPTED PRIVATE KEY":
                    parser = new SSHCOM(binaryData, passPhrase);
                    break;
                case "PuTTY-User-Key-File":
                    var version = privateKeyMatch.Result("${version}");
                    var algorithmName = privateKeyMatch.Result("${algorithmName}");
                    var encryptionType = privateKeyMatch.Result("${encryptionType}");
                    var comment = privateKeyMatch.Result("${comment}");
                    var publicKey = privateKeyMatch.Result("${publicKey}");
                    var argon2Type = privateKeyMatch.Result("${argon2Type}");
                    var argon2Memory = privateKeyMatch.Result("${argon2Memory}");
                    var argon2Passes = privateKeyMatch.Result("${argon2Passes}");
                    var argon2Parallelism = privateKeyMatch.Result("${argon2Parallelism}");
                    var argon2Salt = privateKeyMatch.Result("${argon2Salt}");
                    var mac = privateKeyMatch.Result("${mac}");

                    parser = new PuTTY(
                        version,
                        algorithmName,
                        encryptionType,
                        comment,
                        Convert.FromBase64String(publicKey),
                        argon2Type,
                        argon2Salt,
                        argon2Passes,
                        argon2Memory,
                        argon2Parallelism,
                        binaryData,
                        mac,
                        passPhrase);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", keyName));
            }

            _key = parser.Parse();

            if (_key is RsaKey rsaKey)
            {
                _hostAlgorithms.Add(new KeyHostAlgorithm("ssh-rsa", _key));
#pragma warning disable CA2000 // Dispose objects before losing scope
                _hostAlgorithms.Add(new KeyHostAlgorithm("rsa-sha2-512", _key, new RsaDigitalSignature(rsaKey, HashAlgorithmName.SHA512)));
                _hostAlgorithms.Add(new KeyHostAlgorithm("rsa-sha2-256", _key, new RsaDigitalSignature(rsaKey, HashAlgorithmName.SHA256)));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else
            {
                _hostAlgorithms.Add(new KeyHostAlgorithm(_key.ToString(), _key));
            }
        }

        /// <summary>
        /// Opens the specified certificate.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        private void OpenCertificate(Stream certificate)
        {
            Debug.Assert(certificate is not null, "Should have validated not-null in the constructor.");

            Match certificateMatch;

            using (var sr = new StreamReader(certificate))
            {
                var text = sr.ReadToEnd();
                certificateMatch = CertificateRegex.Match(text);
            }

            if (!certificateMatch.Success)
            {
                throw new SshException("Invalid certificate file.");
            }

            var data = certificateMatch.Result("${data}");

            Certificate = new Certificate(Convert.FromBase64String(data));

            Debug.Assert(Key is not null, $"{nameof(Key)} should have been initialised already.");

            if (!Certificate.Key.Public.SequenceEqual(Key.Public))
            {
                throw new ArgumentException("The supplied certificate does not certify the supplied key.");
            }

            if (Key is RsaKey rsaKey)
            {
                Debug.Assert(Certificate.Key is RsaKey,
                    $"Expected {nameof(Certificate)}.{nameof(Certificate.Key)} to be {nameof(RsaKey)} but was {Certificate.Key?.GetType()}");

                _hostAlgorithms.Insert(0, new CertificateHostAlgorithm("ssh-rsa-cert-v01@openssh.com", Key, Certificate));

#pragma warning disable CA2000 // Dispose objects before losing scope
                _hostAlgorithms.Insert(0, new CertificateHostAlgorithm(
                    "rsa-sha2-256-cert-v01@openssh.com",
                    Key,
                    Certificate,
                    new RsaDigitalSignature(rsaKey, HashAlgorithmName.SHA256)));

                _hostAlgorithms.Insert(0, new CertificateHostAlgorithm(
                    "rsa-sha2-512-cert-v01@openssh.com",
                    Key,
                    Certificate,
                    new RsaDigitalSignature(rsaKey, HashAlgorithmName.SHA512)));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else
            {
                _hostAlgorithms.Insert(0, new CertificateHostAlgorithm(Certificate.Name, Key, Certificate));
            }
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

            if (disposing && _key is IDisposable disposableKey)
            {
                disposableKey.Dispose();

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

        /// <summary>
        /// Represents private key parser.
        /// </summary>
        private interface IPrivateKeyParser
        {
            /// <summary>
            /// Parses the private key.
            /// </summary>
            /// <returns>The <see cref="Key"/>.</returns>
            Key Parse();
        }
    }
}
