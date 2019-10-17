using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements certificate support for host algorithm.
    /// </summary>
    public class CertificateHostAlgorithm : HostAlgorithm
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        public RsaCertificate Certificate { get; private set; }

        /// <summary>
        /// Gets the host key data.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                return new SshCertificateData(Certificate.Name, Certificate.Nonce, Certificate.Exponent, Certificate.Modulus, Certificate.Serial,
                                                Certificate.Type, Certificate.KeyId, Certificate.ValidPrinciples, Certificate.ValidAfter, 
                                                Certificate.ValidBefore, Certificate.CriticalOptions, Certificate.Extensions,
                                                Certificate.SignatureKey, Certificate.Signature).GetBytes();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The host key name.</param>
        public CertificateHostAlgorithm(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The host key name.</param>
        /// <param name="certificate"></param>
        public CertificateHostAlgorithm(string name, RsaCertificate certificate)
            : base(name)
        {
            Certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The host key name.</param>
        /// <param name="data"></param>
        public CertificateHostAlgorithm(string name, byte[] data)
            : base(name)
        {
            var cert = new SshCertificateData();
            cert.Load(data);
            Certificate = cert.Certificate;
        }

        /// <summary>
        /// Signs the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Signed data.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override byte[] Sign(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><c>true</c> if signature was successfully verified; otherwise <c>false</c>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
            throw new NotImplementedException();
        }

        private class SshCertificateData : SshData
        {

            /// <summary>
            /// Gets or sets the name of the algorithm as UTF-8 encoded byte array.
            /// </summary>
            /// <value>
            /// The name of the algorithm.
            /// </value>
            private byte[] _name { get; set; }
            public string Name
            {
                get { return Utf8.GetString(_name, 0, _name.Length); }
                set { _name = Utf8.GetBytes(value); }
            }

            private byte[] _nonce { get; set; }
            public BigInteger Nonce
            {
                get { return new BigInteger(_nonce); }
                set { _nonce = value.ToByteArray(); }
            }

            private byte[] _exponent { get; set; }
            public BigInteger Exponent
            {
                get { return new BigInteger(_exponent); }
                set { _exponent = value.ToByteArray(); }
            }

            private byte[] _modulus { get; set; }
            public BigInteger Modulus
            {
                get { return new BigInteger(_modulus); }
                set { _modulus = value.ToByteArray(); }
            }

            private ulong _serial { get; set; }
            public ulong Serial
            {
                get { return _serial; }
                set { _serial = value; }
            }

            private uint _type { get; set; }
            public uint Type
            {
                get { return _type; }
                set { _type = value; }
            }

            private byte[] _keyId { get; set; }
            public string KeyId
            {
                get { return Utf8.GetString(_keyId, 0, _keyId.Length); }
                set { _keyId = Utf8.GetBytes(value); }
            }

            private byte[] _validPrinciples { get; set; }
            public IList<string> ValidPrinciples
            {
                get
                {
                    var list = new List<string>();
                    var data = new SshDataStream(_validPrinciples);
                    while (!data.IsEndOfData)
                    {
                        list.Add(data.ReadString(Encoding.ASCII));
                    }
                    return list;
                }
                set
                {
                    var data = new SshDataStream(0);
                    foreach(var item in value)
                    {
                        data.Write(item, Encoding.UTF8);
                    }
                    _validPrinciples = data.ToArray();
                }
            }

            private ulong _validAfter { get; set; }
            public DateTime ValidAfter
            {
                get { return _validAfter.FromUnixTime(); }
                set { _validAfter = value.ToUnixTime(); }
            }

            private ulong _validBefore { get; set; }
            public DateTime ValidBefore
            {
                get { return _validBefore.FromUnixTime(); }
                set { _validBefore = value.ToUnixTime(); }
            }

            private byte[] _criticalOptions { get; set; }
            public IList<string> CriticalOptions
            {
                get
                {
                    var list = new List<string>();
                    var data = new SshDataStream(_criticalOptions);
                    while (!data.IsEndOfData)
                    {
                        list.Add(data.ReadString(Encoding.ASCII));
                    }
                    return list;
                }
                set
                {
                    var data = new SshDataStream(0);
                    foreach (var item in value)
                    {
                        data.Write(item, Encoding.UTF8);
                    }
                    _criticalOptions = data.ToArray();
                }
            }

            private byte[] _extensions { get; set; }
            public IList<string> Extensions
            {
                get
                {
                    var list = new List<string>();
                    var data = new SshDataStream(_extensions);
                    while (!data.IsEndOfData)
                    {
                        list.Add(data.ReadString(Encoding.ASCII));
                    }
                    return list;
                }
                set
                {
                    var data = new SshDataStream(0);
                    foreach (var item in value)
                    {
                        data.Write(item, Encoding.UTF8);
                    }
                    _extensions = data.ToArray();
                }
            }

            private byte[] _reserved { get; set; }

            private byte[] _signatureKey { get; set; }
            public BigInteger SignatureKey
            {
                get { return new BigInteger(_signatureKey); }
                set { _signatureKey = value.ToByteArray(); }
            }

            private byte[] _signature { get; set; }
            public BigInteger Signature
            {
                get { return new BigInteger(_signature); }
                set { _signature = value.ToByteArray(); }
            }

            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // AlgorithmName length
                    capacity += _name.Length; // AlgorithmName
                    capacity += 4; // Nonce length
                    capacity += _nonce.Length; // Signature
                    capacity += 4;
                    capacity += _exponent.Length;
                    capacity += 4;
                    capacity += _modulus.Length;
                    capacity += sizeof(ulong); // Serial
                    capacity += sizeof(uint); // Type
                    capacity += 4;
                    capacity += _keyId.Length; // Key Id
                    capacity += 4;
                    capacity += _validPrinciples.Length;
                    capacity += sizeof(ulong); // Valid After
                    capacity += sizeof(ulong); // Valid Before
                    capacity += 4;
                    capacity += _criticalOptions.Length;
                    capacity += 4;
                    capacity += _extensions.Length;
                    capacity += 4;
                    capacity += _reserved.Length;
                    capacity += 4;
                    capacity += _signatureKey.Length;
                    capacity += 4;
                    capacity += _signature.Length;

                    return capacity;
                }
            }

            public SshCertificateData()
            {
                _reserved = new byte[0];
            }

            public SshCertificateData(string name, BigInteger nonce, BigInteger exponent, BigInteger modulus, ulong serial, uint type, string keyId, 
                                        IList<string> validPrinciples, DateTime validAfter, DateTime validBefore, IList<string> criticalOptions,
                                        IList<string> extensions, BigInteger signatureKey, BigInteger signature) : this()
            {
                Name = name;
                Nonce = nonce;
                Exponent = exponent;
                Modulus = modulus;
                Serial = serial;
                Type = type;
                KeyId = keyId;
                ValidPrinciples = validPrinciples;
                ValidAfter = validAfter;
                ValidBefore = validBefore;
                CriticalOptions = criticalOptions;
                Extensions = extensions;
                SignatureKey = signatureKey;
                Signature = signature;
            }

            public RsaCertificate Certificate
            {
                get
                {
                    return new RsaCertificate(Name, Nonce, Exponent, Modulus, Serial, Type, KeyId, ValidPrinciples,
                                                ValidAfter, ValidBefore, Extensions, CriticalOptions, SignatureKey, Signature);
                }
            }

            /// <summary>
            /// Called when type specific data need to be loaded.
            /// </summary>
            protected override void LoadData()
            {
                _name = ReadBinary();
                _nonce = ReadBinary();
                _exponent = ReadBinary();
                _modulus = ReadBinary();
                _serial = ReadUInt64();
                _type = ReadUInt32();
                _keyId = ReadBinary();
                _validPrinciples = ReadBinary();
                _validAfter = ReadUInt64();
                _validBefore = ReadUInt64();
                _criticalOptions = ReadBinary();
                _extensions = ReadBinary();
                _reserved = ReadBinary();
                _signatureKey = ReadBinary();
                _signature = ReadBinary();
            }

            /// <summary>
            /// Called when type specific data need to be saved.
            /// </summary>
            protected override void SaveData()
            {
                WriteBinaryString(_name);
                WriteBinaryString(_nonce);
                WriteBinaryString(_exponent);
                WriteBinaryString(_modulus);
                Write(_serial);
                Write(_type);
                WriteBinaryString(_keyId);
                WriteBinaryString(_validPrinciples);
                Write(_validAfter);
                Write(_validBefore);
                WriteBinaryString(_criticalOptions);
                WriteBinaryString(_extensions);
                WriteBinaryString(_reserved);
                WriteBinaryString(_signatureKey);
                WriteBinaryString(_signature);
            }
        }
    }
}
