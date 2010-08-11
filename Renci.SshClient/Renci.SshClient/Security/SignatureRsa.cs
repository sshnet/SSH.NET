using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    internal class SignatureRsa : Signature
    {
        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        public SignatureRsa(IEnumerable<byte> data)
            : base(data)
        {

        }

        public override bool ValidateSignature(IEnumerable<byte> hash, IEnumerable<byte> signature)
        {
            var exponentLength = BitConverter.ToUInt32(this.Data.Take(4).Reverse().ToArray(), 0);

            var exponentData = this.Data.Skip(4).Take((int)exponentLength).ToArray();

            var modulusLength = BitConverter.ToUInt32(this.Data.Skip(4 + (int)exponentLength).Take(4).Reverse().ToArray(), 0);

            var modulusData = this.Data.Skip(4 + (int)exponentLength + 4).Take((int)modulusLength).ToArray();

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                using (var cs = new CryptoStream(System.IO.Stream.Null, sha1, CryptoStreamMode.Write))
                {
                    var data = hash.ToArray();
                    cs.Write(data, 0, data.Length);
                    cs.Close();
                }

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(new RSAParameters
                    {
                        Exponent = exponentData,
                        Modulus = modulusData.TrimLeadinZero().ToArray(),
                    });
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA1");

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

                    return rsaDeformatter.VerifySignature(sha1, sig);
                }
            }
        }
    }
}
