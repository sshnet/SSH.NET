using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    public class RequestMessagePublicKey : RequestMessage
    {
        public override string MethodName
        {
            get
            {
                return "publickey";
            }
        }

        public string PublicKeyAlgorithmName { get; private set; }

        public IEnumerable<byte> PublicKeyData { get; private set; }

        public IEnumerable<byte> Signature { get; set; }

        public RequestMessagePublicKey(ServiceNames serviceName, string username, string keyAlgorithmName, IEnumerable<byte> keyData)
            : base(serviceName, username)
        {
            this.PublicKeyAlgorithmName = keyAlgorithmName;
            this.PublicKeyData = keyData;
        }

        public RequestMessagePublicKey(ServiceNames serviceName, string username, string keyAlgorithmName, IEnumerable<byte> keyData, IEnumerable<byte> signature)
            : this(serviceName, username, keyAlgorithmName, keyData)
        {
            this.Signature = signature;
        }

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
