using System.Collections.Generic;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements key support for host algorithm.
    /// </summary>
    public class KeyHostAlgorithm : HostAlgorithm
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        public Key Key { get; private set; }

        /// <summary>
        /// Gets the public key data.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                return new SshKeyData(Name, Key.Public).GetBytes();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">Host key name.</param>
        /// <param name="key">Host key.</param>
        public KeyHostAlgorithm(string name, Key key)
            : base(name)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">Host key name.</param>
        /// <param name="key">Host key.</param>
        /// <param name="data">Host key encoded data.</param>
        public KeyHostAlgorithm(string name, Key key, byte[] data)
            : base(name)
        {
            Key = key;

            var sshKey = new SshKeyData();
            sshKey.Load(data);
            Key.Public = sshKey.Keys;
        }

        /// <summary>
        /// Signs the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// Signed data.
        /// </returns>
        public override byte[] Sign(byte[] data)
        {
            return new SignatureKeyData(Name, Key.Sign(data)).GetBytes();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        ///   <c>True</c> is signature was successfully verifies; otherwise <c>false</c>.
        /// </returns>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
            var signatureData = new SignatureKeyData();
            signatureData.Load(signature);

            return Key.VerifySignature(data, signatureData.Signature);
        }

        private class SshKeyData : SshData
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
                        _keys.Add(key.ToByteArray().Reverse());
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

        private class SignatureKeyData : SshData
        {
            /// <summary>
            /// Gets or sets the name of the algorithm as UTF-8 encoded byte array.
            /// </summary>
            /// <value>
            /// The name of the algorithm.
            /// </value>
            private byte[] AlgorithmName { get; set; }

            /// <summary>
            /// Gets or sets the signature.
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
                    capacity += AlgorithmName.Length; // AlgorithmName
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
                AlgorithmName = Utf8.GetBytes(name);
                Signature = signature;
            }

            /// <summary>
            /// Called when type specific data need to be loaded.
            /// </summary>
            protected override void LoadData()
            {
                AlgorithmName = ReadBinary();
                Signature = ReadBinary();
            }

            /// <summary>
            /// Called when type specific data need to be saved.
            /// </summary>
            protected override void SaveData()
            {
                WriteBinaryString(AlgorithmName);
                WriteBinaryString(Signature);
            }
        }
    }
}
