using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    internal class CryptoPublicKeyDss : CryptoPublicKey
    {
        private IEnumerable<byte> _p;
        private IEnumerable<byte> _q;
        private IEnumerable<byte> _g;
        private IEnumerable<byte> _publicKey;

        public override string Name
        {
            get { return "ssh-dss"; }
        }

        public CryptoPublicKeyDss()
        {

        }

        public CryptoPublicKeyDss(IEnumerable<byte> p, IEnumerable<byte> q, IEnumerable<byte> g, IEnumerable<byte> publicKey)
        {
            this._p = p;
            this._q = q;
            this._g = g;
            this._publicKey = publicKey;
        }

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
                }
            }
        }

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
                        Y = _publicKey.TrimLeadinZero().ToArray(),
                        P = _p.TrimLeadinZero().ToArray(),
                        Q = _q.TrimLeadinZero().ToArray(),
                        G = _g.TrimLeadinZero().ToArray(),
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
