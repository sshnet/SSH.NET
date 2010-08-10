using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    internal abstract class PrivateKey
    {
        public abstract string AlgorithmName { get; }

        protected IEnumerable<byte> Data { get; private set; }

        public abstract IEnumerable<byte> PublicKey { get; }

        public PrivateKey(IEnumerable<byte> data)
        {
            this.Data = data;
        }

        public abstract IEnumerable<byte> GetSignature(IEnumerable<byte> sessionId);

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
