using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    public class CryptoPrivateKeyRsa : CryptoPrivateKey
    {
        private byte[] _modulus;
        private byte[] _exponent;
        private byte[] _dValue;
        private byte[] _pValue;
        private byte[] _qValue;
        private byte[] _dpValue;
        private byte[] _dqValue;
        private byte[] _inverseQ;

        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        public override void Load(IEnumerable<byte> data)
        {
            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            using (var ms = new MemoryStream(data.ToArray()))
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
                    throw new InvalidOperationException("Not valid RSA Key.");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	//version number
                    throw new NotSupportedException("Not supported RSA Key version.");
                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new InvalidOperationException("Not valid RSA Key.");


                //------  all private key components are Integer sequences ----
                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._modulus = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._exponent = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._dValue = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._pValue = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._qValue = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._dpValue = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._dqValue = binr.ReadBytes(elems);

                elems = CryptoPrivateKeyRsa.GetIntegerSize(binr);
                this._inverseQ = binr.ReadBytes(elems);
            }
        }

        public override CryptoPublicKey GetPublicKey()
        {
            return new CryptoPublicKeyRsa(this._modulus, this._exponent);
        }

        public override IEnumerable<byte> GetSignature(IEnumerable<byte> key)
        {
            var data = key.ToArray();
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, sha1, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                RSAParameters RSAKeyInfo = new RSAParameters();

                RSAKeyInfo.Exponent = _exponent.TrimLeadinZero().ToArray();
                RSAKeyInfo.D = _dValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.Modulus = _modulus.TrimLeadinZero().ToArray();
                RSAKeyInfo.P = _pValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.Q = _qValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.DP = _dpValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.DQ = _dqValue.TrimLeadinZero().ToArray();
                RSAKeyInfo.InverseQ = _inverseQ.TrimLeadinZero().ToArray();

                cs.Write(data, 0, data.Length);

                cs.Close();

                var RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKeyInfo);
                var RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                RSAFormatter.SetHashAlgorithm("SHA1");

                var signature = RSAFormatter.CreateSignature(sha1);

                return new SignatureKeyData
                {
                    AlgorithmName = this.Name,
                    Signature = signature,
                }.GetBytes();
            }
        }

        public override bool VerifySignature(IEnumerable<byte> hash, IEnumerable<byte> signature)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<byte> GetBytes()
        {
            throw new NotImplementedException();
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
    }
}
