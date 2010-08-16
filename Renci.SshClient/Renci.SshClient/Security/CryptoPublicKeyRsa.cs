using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    public class CryptoPublicKeyRsa : CryptoPublicKey
    {
        private IEnumerable<byte> _modulus;
        private IEnumerable<byte> _exponent;

        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        public CryptoPublicKeyRsa()
        {

        }

        internal CryptoPublicKeyRsa(IEnumerable<byte> modulus, IEnumerable<byte> exponent)
        {
            this._modulus = modulus;
            this._exponent = exponent;
        }

        public override void Load(IEnumerable<byte> data)
        {
            using (var ms = new MemoryStream(data.ToArray()))
            using (var br = new BinaryReader(ms))
            {

                var el = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                this._exponent = br.ReadBytes((int)el);

                var ml = BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                this._modulus = br.ReadBytes((int)ml);
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

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(new RSAParameters
                    {
                        Exponent = this._exponent.TrimLeadinZero().ToArray(),
                        Modulus = this._modulus.TrimLeadinZero().ToArray(),
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

        public override IEnumerable<byte> GetBytes()
        {
            return new RsaPublicKeyData
            {
                E = this._exponent,
                Modulus = this._modulus,
            }.GetBytes();
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
