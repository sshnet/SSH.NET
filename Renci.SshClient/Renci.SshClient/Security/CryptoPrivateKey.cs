using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    public abstract class CryptoPrivateKey : CryptoKey
    {
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
