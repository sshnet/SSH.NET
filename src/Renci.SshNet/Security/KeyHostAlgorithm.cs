using System.Collections.Generic;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Chaos.NaCl;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements key support for host algorithm.
    /// </summary>
    public class KeyHostAlgorithm : HostAlgorithm
    {
        /// <summary>
        /// Gets the key used in this host key algorithm.
        /// </summary>
        /// <value>
        /// The key used in this host key algorithm.
        /// </value>
        public Key Key { get; private set; }

        /// <summary>
        /// Gets the signature implementation used in this host key algorithm.
        /// </summary>
        /// <value>
        /// The signature implementation used in this host key algorithm.
        /// </value>
        public DigitalSignature DigitalSignature { get; private set; }

        /// <summary>
        /// Gets the encoded public key data.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                var keyFormatIdentifier = Key is RsaKey ? "ssh-rsa" : Name;
                return new SshKeyData(keyFormatIdentifier, Key.Public).GetBytes();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
        /// <remarks>
        /// This constructor is typically passed a private key in order to create an encoded signature for later
        /// verification by the host.
        /// </remarks>
        public KeyHostAlgorithm(string name, Key key)
            : base(name)
        {
            Key = key;
            DigitalSignature = key.DigitalSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
        /// <param name="digitalSignature"><inheritdoc cref="DigitalSignature" path="/summary"/></param>
        /// <remarks>
        /// <para>
        /// This constructor is typically passed a private key in order to create an encoded signature for later
        /// verification by the host.
        /// </para>
        /// The key used by <paramref name="digitalSignature"/> is intended to be equal to <paramref name="key"/>.
        /// This is not verified.
        /// </remarks>
        public KeyHostAlgorithm(string name, Key key, DigitalSignature digitalSignature)
            : base(name)
        {
            Key = key;
            DigitalSignature = digitalSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class
        /// with the given encoded public key data. The data will be decoded into <paramref name="key"/>.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
        /// <param name="data">Host key encoded data.</param>
        /// <remarks>
        /// This constructor is typically passed a new or reusable <see cref="Security.Key"/> instance in
        /// order to verify an encoded signature sent by the host, created by the private counterpart
        /// to the host's public key, which is encoded in <paramref name="data"/>.
        /// </remarks>
        public KeyHostAlgorithm(string name, Key key, byte[] data)
            : base(name)
        {
            Key = key;

            var sshKey = new SshKeyData();
            sshKey.Load(data);
            Key.Public = sshKey.Keys;

            DigitalSignature = key.DigitalSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class
        /// with the given encoded public key data. The data will be decoded into <paramref name="key"/>.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
        /// <param name="data">Host key encoded data.</param>
        /// <param name="digitalSignature"><inheritdoc cref="DigitalSignature" path="/summary"/></param>
        /// <remarks>
        /// <para>
        /// This constructor is typically passed a new or reusable <see cref="Security.Key"/> instance in
        /// order to verify an encoded signature sent by the host, created by the private counterpart
        /// to the host's public key, which is encoded in <paramref name="data"/>.
        /// </para>
        /// The key used by <paramref name="digitalSignature"/> is intended to be equal to <paramref name="key"/>.
        /// This is not verified.
        /// </remarks>
        public KeyHostAlgorithm(string name, Key key, byte[] data, DigitalSignature digitalSignature)
            : base(name)
        {
            Key = key;

            var sshKey = new SshKeyData();
            sshKey.Load(data);
            Key.Public = sshKey.Keys;

            DigitalSignature = digitalSignature;
        }

        /// <summary>
        /// Signs and encodes the specified data.
        /// </summary>
        /// <param name="data">The data to be signed.</param>
        /// <returns>
        /// The encoded signature.
        /// </returns>
        public override byte[] Sign(byte[] data)
        {
            return new SignatureKeyData(Name, DigitalSignature.Sign(data)).GetBytes();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data to verify the signature against.</param>
        /// <param name="signature">The encoded signature data.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="signature"/> is the result of signing <paramref name="data"/>
        /// with the corresponding private key to <see cref="Key"/>.
        /// </returns>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
            var signatureData = new SignatureKeyData();
            signatureData.Load(signature);

            return DigitalSignature.Verify(data, signatureData.Signature);
        }

        private sealed class SshKeyData : SshData
        {
            private byte[] _name;
            private List<byte[]> _keys;

            public BigInteger[] Keys
            {
                get
                {
                    var keys = new BigInteger[_keys.Count];

                    for (var i = 0; i < _keys.Count; i++)
                    {
                        var key = _keys[i];
                        keys[i] = key.ToBigInteger2();
                    }

                    return keys;
                }
                private set
                {
                    _keys = new List<byte[]>(value.Length);

                    foreach (var key in value)
                    {
                        var keyData = key.ToByteArray().Reverse();
                        if (Name == "ssh-ed25519")
                        {
                            keyData = keyData.TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
                        }

                        _keys.Add(keyData);
                    }
                }
            }

            private string Name
            {
                get { return Utf8.GetString(_name, 0, _name.Length); }
                set { _name = Utf8.GetBytes(value); }
            }

            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // Name length
                    capacity += _name.Length; // Name

                    foreach (var key in _keys)
                    {
                        capacity += 4; // Key length
                        capacity += key.Length; // Key
                    }

                    return capacity;
                }
            }

            public SshKeyData()
            {
            }

            public SshKeyData(string name, params BigInteger[] keys)
            {
                Name = name;
                Keys = keys;
            }

            protected override void LoadData()
            {
                _name = ReadBinary();
                _keys = new List<byte[]>();

                while (!IsEndOfData)
                {
                    _keys.Add(ReadBinary());
                }
            }

            protected override void SaveData()
            {
                WriteBinaryString(_name);

                foreach (var key in _keys)
                {
                    WriteBinaryString(key);
                }
            }
        }

        internal sealed class SignatureKeyData : SshData
        {
            /// <summary>
            /// Gets or sets the signature format identifier.
            /// </summary>
            public string AlgorithmName { get; set; }

            /// <summary>
            /// Gets the signature.
            /// </summary>
            /// <value>
            /// The signature.
            /// </value>
            public byte[] Signature { get; private set; }

            /// <summary>
            /// Gets the size of the message in bytes.
            /// </summary>
            /// <value>
            /// The size of the messages in bytes.
            /// </value>
            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // AlgorithmName length
                    capacity += Encoding.UTF8.GetByteCount(AlgorithmName); // AlgorithmName
                    capacity += 4; // Signature length
                    capacity += Signature.Length; // Signature
                    return capacity;
                }
            }

            public SignatureKeyData()
            {
            }

            public SignatureKeyData(string name, byte[] signature)
            {
                AlgorithmName = name;
                Signature = signature;
            }

            /// <summary>
            /// Called when type specific data need to be loaded.
            /// </summary>
            protected override void LoadData()
            {
                AlgorithmName = Encoding.UTF8.GetString(ReadBinary());
                Signature = ReadBinary();
            }

            /// <summary>
            /// Called when type specific data need to be saved.
            /// </summary>
            protected override void SaveData()
            {
                WriteBinaryString(Encoding.UTF8.GetBytes(AlgorithmName));
                WriteBinaryString(Signature);
            }
        }
    }
}
