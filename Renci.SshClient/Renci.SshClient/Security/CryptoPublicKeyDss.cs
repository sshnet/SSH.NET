using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    public class CryptoPublicKeyDss : CryptoPublicKey
    {
        private IEnumerable<byte> _p;
        private IEnumerable<byte> _q;
        private IEnumerable<byte> _g;
        private IEnumerable<byte> _x;

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public CryptoPublicKeyDss()
        {

        }

        public CryptoPublicKeyDss(IEnumerable<byte> p, IEnumerable<byte> q, IEnumerable<byte> g, IEnumerable<byte> x)
        {
            this._p = p;
            this._q = q;
            this._g = g;
            this._x = x;
        }

        public override void Load(IEnumerable<byte> data)
        {
            using (var ms = new MemoryStream(data.ToArray()))
            using (var br = new BinaryReader(ms))
            {

                var pl = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                _p = br.ReadBytes((int)pl);

                var ql = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                _q = br.ReadBytes((int)ql);

                var gl = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                _g = br.ReadBytes((int)gl);

                var xl = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                _x = br.ReadBytes((int)xl);
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
                    cs.Close();
                }

                using (var dsa = new DSACryptoServiceProvider())
                {
                    dsa.ImportParameters(new DSAParameters
                    {
                        X = _x.TrimLeadinZero().ToArray(),
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
            throw new NotImplementedException();
        }
    }
}
