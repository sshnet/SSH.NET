using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    internal class PublicKeyRequestMessage : RequestMessage
    {
        public override string MethodName
        {
            get
            {
                return "publickey";
            }
        }

        public string PublicKeyAlgorithmName { get; set; }

        public IEnumerable<byte> PublicKeyData { get; set; }

        public IEnumerable<byte> Signature { get; set; }

        protected override void SaveData()
        {
            base.SaveData();

            if (this.Signature == null)
            {
                this.Write(false);
            }
            else
            {
                this.Write(true);
            }
            this.Write(this.PublicKeyAlgorithmName);
            this.Write(this.PublicKeyData.GetSshString());
            if (this.Signature != null)
                this.Write(this.Signature.GetSshString());
        }
    }
}
