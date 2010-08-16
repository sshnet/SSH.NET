using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    public abstract class CryptoPrivateKey : CryptoKey
    {
        public override void Load(IEnumerable<byte> data)
        {
            this.Load(data, null);
        }

        public abstract void Load(IEnumerable<byte> data, IEnumerable<byte> passPhrase);

        public abstract CryptoPublicKey GetPublicKey();

        public abstract IEnumerable<byte> GetSignature(IEnumerable<byte> key);

        protected class SignatureKeyData : SshData
        {
            public string AlgorithmName { get; set; }

            public IEnumerable<byte> Signature { get; set; }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                this.Write(this.AlgorithmName);
                this.Write(this.Signature.GetSshString());
            }
        }
    }
}
