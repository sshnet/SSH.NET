using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    internal class PrivateKeyDsa : PrivateKey
    {
        private byte[] _pValue;

        private byte[] _qValue;

        private byte[] _gValue;

        private byte[] _publicKeyValue;

        private byte[] _privateKeyValue;

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
                    this._publicKey = new DsaPublicKeyData
                    {
                        P = this._pValue,
                        Q = this._qValue,
                        G = this._gValue,
                        Public = this._publicKeyValue,
                    }.GetBytes();

                }
                return this._publicKey;
            }
        }

        public override string AlgorithmName
        {
            get { return "ssh-dss"; }
        }


        public PrivateKeyDsa(IEnumerable<byte> data)
            : base(data)
        {
            if (!this.ParseDSAPrivateKey())
            {
                throw new InvalidDataException("DSA Key is not valid");
            }
        }

        public override IEnumerable<byte> GetSignature(IEnumerable<byte> sessionId)
        {
            var data = sessionId.ToArray();
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, sha1, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                DSAParameters DSAKeyInfo = new DSAParameters();

                DSAKeyInfo.X = this._privateKeyValue.TrimLeadinZero().ToArray();
                DSAKeyInfo.P = this._pValue.TrimLeadinZero().ToArray();
                DSAKeyInfo.Q = this._qValue.TrimLeadinZero().ToArray();
                DSAKeyInfo.G = this._gValue.TrimLeadinZero().ToArray();

                cs.Write(data, 0, data.Length);

                cs.Close();

                var DSA = new System.Security.Cryptography.DSACryptoServiceProvider();
                DSA.ImportParameters(DSAKeyInfo);
                var DSAFormatter = new RSAPKCS1SignatureFormatter(DSA);
                DSAFormatter.SetHashAlgorithm("SHA1");

                var signature = DSAFormatter.CreateSignature(sha1);

                return new SignatureKeyData
                {
                    AlgorithmName = this.AlgorithmName,
                    Signature = signature,
                }.GetBytes();
            }

        }

        private bool ParseDSAPrivateKey()
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
                this._pValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._qValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._gValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._publicKeyValue = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                this._privateKeyValue = binr.ReadBytes(elems);
            }

            return true;
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

        private class DsaPublicKeyData : SshData
        {
            public IEnumerable<byte> P { get; set; }

            public IEnumerable<byte> Q { get; set; }

            public IEnumerable<byte> G { get; set; }

            public IEnumerable<byte> Public { get; set; }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                this.Write("ssh-dss");
                this.Write(this.P.GetSshString());
                this.Write(this.Q.GetSshString());
                this.Write(this.G.GetSshString());
                this.Write(this.Public.GetSshString());
            }
        }
    }
}
