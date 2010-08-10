using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    internal class PrivateKeyRsa : PrivateKey
    {
        private byte[] _modulus;
        private byte[] _eValue;
        private byte[] _dValue;
        private byte[] _pValue;
        private byte[] _qValue;
        private byte[] _dpValue;
        private byte[] _dqValue;
        private byte[] _iqValue;

        private IEnumerable<byte> _publicKey;
        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <value>The public key.</value>
        public override IEnumerable<byte> PublicKey
        {
            get
            {
                if (this._publicKey == null)
                {
                    this._publicKey = new RsaPublicKeyData
                    {
                        E = this._eValue,
                        Modulus = this._modulus,
                    }.GetBytes();

                }
                return this._publicKey;
            }
        }

        public override string AlgorithmName
        {
            get { return "ssh-rsa"; }
        }


        public PrivateKeyRsa(IEnumerable<byte> data)
            : base(data)
        {
            if (!this.ParseRSAPrivateKey())
            {
                throw new InvalidDataException("RSA Key is not valid");
            }
        }

        public override IEnumerable<byte> GetSignature(IEnumerable<byte> sessionId)
        {
            var data = sessionId.ToArray();
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, sha1, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                RSAParameters RSAKeyInfo = new RSAParameters();

                RSAKeyInfo.Exponent = _eValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.D = _dValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.Modulus = _modulus.TrimLeadinZero().ToArray();
                RSAKeyInfo.P = _pValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.Q = _qValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.DP = _dpValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.DQ = _dqValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.InverseQ = _iqValue.TrimLeadinZero().ToArray();

                cs.Write(data, 0, data.Length);

                cs.Close();

                var RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKeyInfo);
                var RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                RSAFormatter.SetHashAlgorithm("SHA1");

                var signature = RSAFormatter.CreateSignature(sha1);

                return new SignatureKeyData
                {
                    AlgorithmName = this.AlgorithmName,
                    Signature = signature,
                }.GetBytes();
            }

        }

        private bool ParseRSAPrivateKey()
        {
            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            using (var ms = new MemoryStream(this.Data.ToArray()))
            using (var binr = new BinaryReader(ms)) //wrap Memory Stream with BinaryReader for easy reading
            {
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return false;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	//version number
                    return false;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return false;


                //------  all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                this._modulus = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._eValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._dValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._pValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._qValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._dpValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._dqValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._iqValue = binr.ReadBytes(elems);

                return true;
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)		//expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte();	// data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;		// we already have the data size
                }

            return count;
        }

        private class RsaPublicKeyData : SshData
        {
            public IEnumerable<byte> Modulus { get; set; }

            public IEnumerable<byte> E { get; set; }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                this.Write("ssh-rsa");
                this.Write(this.E.GetSshString());
                this.Write(this.Modulus.GetSshString());
            }
        }
    }
}
