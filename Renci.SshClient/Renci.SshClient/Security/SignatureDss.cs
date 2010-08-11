using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    internal class SignatureDss : Signature
    {
        public override string Name
        {
            get { return "ssh-dss"; }
        }

        public SignatureDss(IEnumerable<byte> data)
            : base(data)
        {

        }

        public override bool ValidateSignature(IEnumerable<byte> hash, IEnumerable<byte> signature)
        {
            var pLength = BitConverter.ToUInt32(this.Data.Take(4).Reverse().ToArray(), 0);

            var pData = this.Data.Skip(4).Take((int)pLength).ToArray();

            var qLength = BitConverter.ToUInt32(this.Data.Skip(4 + (int)pLength).Take(4).Reverse().ToArray(), 0);

            var qData = this.Data.Skip(4 + (int)pLength + 4).Take((int)qLength).ToArray();

            var gLength = BitConverter.ToUInt32(this.Data.Skip(4 + (int)pLength + 4 + (int)qLength).Take(4).Reverse().ToArray(), 0);

            var gData = this.Data.Skip(4 + (int)pLength + 4 + (int)qLength + 4).Take((int)gLength).ToArray();

            var xLength = BitConverter.ToUInt32(this.Data.Skip(4 + (int)pLength + 4 + (int)qLength + 4 + (int)gLength).Take(4).Reverse().ToArray(), 0);

            var xData = this.Data.Skip(4 + (int)pLength + 4 + (int)qLength + 4 + (int)xLength + 4).Take((int)xLength).ToArray();

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
                        X = xData.TrimLeadinZero().ToArray(),
                        P = pData.TrimLeadinZero().ToArray(),
                        Q = qData.TrimLeadinZero().ToArray(),
                        G = gData.TrimLeadinZero().ToArray(),
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
    }
}
