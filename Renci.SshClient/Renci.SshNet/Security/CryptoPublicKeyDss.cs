using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents DSS public key
    /// </summary>
    internal class CryptoPublicKeyDss : CryptoPublicKey
    {
        private byte[] _p;
        private byte[] _q;
        private byte[] _g;
        private byte[] _publicKey;

        /// <summary>
        /// Gets key name.
        /// </summary>
        public override string Name
        {
            get { return "ssh-dss"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoPublicKeyDss"/> class.
        /// </summary>
        public CryptoPublicKeyDss()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoPublicKeyDss"/> class.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <param name="q">The q value.</param>
        /// <param name="g">The g value.</param>
        /// <param name="publicKey">The public key value.</param>
        public CryptoPublicKeyDss(byte[] p, byte[] q, byte[] g, byte[] publicKey)
        {
            this._p = p;
            this._q = q;
            this._g = g;
            this._publicKey = publicKey;
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
                using (var br = new BinaryReader(ms))
                {

                    var pl = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    _p = br.ReadBytes((int)pl);

                    var ql = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    _q = br.ReadBytes((int)ql);

                    var gl = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    _g = br.ReadBytes((int)gl);

                    var xl = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    _publicKey = br.ReadBytes((int)xl);
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
        /// Verifies the signature.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// true if signature verified; otherwise false.
        /// </returns>
        public override bool VerifySignature(IEnumerable<byte> hash, IEnumerable<byte> signature)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                using (var cs = new CryptoStream(System.IO.Stream.Null, sha1, CryptoStreamMode.Write))
                {
                    var data = hash.ToArray();
                    cs.Write(data, 0, data.Length);
                }

                using (var dsa = new DSACryptoServiceProvider())
                {
                    dsa.ImportParameters(new DSAParameters
                    {
                        Y = _publicKey.TrimLeadingZero().ToArray(),
                        P = _p.TrimLeadingZero().ToArray(),
                        Q = _q.TrimLeadingZero().ToArray(),
                        G = _g.TrimLeadingZero().ToArray(),
                    });
                    var dsaDeformatter = new DSASignatureDeformatter(dsa);
                    dsaDeformatter.SetHashAlgorithm("SHA1");

                    long i = 0;
                    long j = 0;
                    byte[] tmp;

                    var sig = signature.ToArray();
                    if (sig[0] == 0 && sig[1] == 0 && sig[2] == 0)
                    {
                        long i1 = (sig[i++] << 24) & 0xff000000;
                        long i2 = (sig[i++] << 16) & 0x00ff0000;
                        long i3 = (sig[i++] << 8) & 0x0000ff00;
                        long i4 = (sig[i++]) & 0x000000ff;
                        j = i1 | i2 | i3 | i4;

                        i += j;

                        i1 = (sig[i++] << 24) & 0xff000000;
                        i2 = (sig[i++] << 16) & 0x00ff0000;
                        i3 = (sig[i++] << 8) & 0x0000ff00;
                        i4 = (sig[i++]) & 0x000000ff;
                        j = i1 | i2 | i3 | i4;

                        tmp = new byte[j];
                        Array.Copy(sig, i, tmp, 0, j);
                        sig = tmp;
                    }

                    return dsaDeformatter.VerifySignature(sha1, sig);
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
            return new DsaPublicKeyData
            {
                P = this._p,
                Q = this._q,
                G = this._g,
                Public = this._publicKey,
            }.GetBytes();
        }

        private class DsaPublicKeyData : SshData
        {
            public byte[] P { get; set; }

            public byte[] Q { get; set; }

            public byte[] G { get; set; }

            public byte[] Public { get; set; }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                this.Write("ssh-dss");
                this.WriteBinaryString(this.P);
                this.WriteBinaryString(this.Q);
                this.WriteBinaryString(this.G);
                this.WriteBinaryString(this.Public);
            }
        }
    }
}
