using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents RSA private key
    /// </summary>
    internal class CryptoPrivateKeyRsa : CryptoPrivateKey
    {
        private byte[] _modulus;
        private byte[] _exponent;
        private byte[] _dValue;
        private byte[] _pValue;
        private byte[] _qValue;
        private byte[] _dpValue;
        private byte[] _dqValue;
        private byte[] _inverseQ;

        /// <summary>
        /// Gets key name.
        /// </summary>
        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        /// <summary>
        /// Loads key specific data.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Load(IEnumerable<byte> data)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream(data.ToArray());
                using (var binr = new BinaryReader(ms))
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
                        throw new SshException("RSA key is not valid for use in specified state");

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102)	//version number
                        throw new SshException("RSA key version is not supported.");
                    
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new SshException("RSA key is not valid for use in specified state");

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
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                }
            }
        }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <returns></returns>
        public override CryptoPublicKey GetPublicKey()
        {
            return new CryptoPublicKeyRsa(this._modulus, this._exponent);
        }

        /// <summary>
        /// Gets the signature.
        /// </summary>
        /// <param name="key">The key data bytes.</param>
        /// <returns></returns>
        public override byte[] GetSignature(IEnumerable<byte> key)
        {
            var data = key.ToArray();
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                RSAParameters rsaKeyInfo = new RSAParameters();

                using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, sha1, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    rsaKeyInfo.Exponent = this._exponent.TrimLeadingZero().ToArray();
                    rsaKeyInfo.D = this._dValue.TrimLeadingZero().ToArray();
                    rsaKeyInfo.Modulus = this._modulus.TrimLeadingZero().ToArray();
                    rsaKeyInfo.P = this._pValue.TrimLeadingZero().ToArray();
                    rsaKeyInfo.Q = this._qValue.TrimLeadingZero().ToArray();
                    rsaKeyInfo.DP = this._dpValue.TrimLeadingZero().ToArray();
                    rsaKeyInfo.DQ = this._dqValue.TrimLeadingZero().ToArray();
                    rsaKeyInfo.InverseQ = this._inverseQ.TrimLeadingZero().ToArray();

                    cs.Write(data, 0, data.Length);
                }

                using (var RSA = new System.Security.Cryptography.RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(rsaKeyInfo);
                    var RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                    RSAFormatter.SetHashAlgorithm("SHA1");

                    var signature = RSAFormatter.CreateSignature(sha1);

                    return new SignatureKeyData
                    {
                        AlgorithmName = this.Name,
                        Signature = signature,
                    }.GetBytes().ToArray();
                }
            }
        }

        /// <summary>
        /// Gets key data byte array.
        /// </summary>
        /// <returns>
        /// The data byte array.
        /// </returns>
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
                    count = (int)(modint[3] << 24 | modint[2] << 16 | modint[1] << 8 | modint[0]);
                }
                else
                {
                    count = bt;		// we already have the data size
                }

            return count;
        }
    }
}
