using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    internal class PublicKeyMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationPublicKey; }
        }

        public string PublicKeyAlgorithmName { get; private set; }

        public IEnumerable<byte> PublicKeyData { get; private set; }

        protected override void LoadData()
        {
            this.PublicKeyAlgorithmName = this.ReadString();
            this.PublicKeyData = this.ReadString().GetSshBytes();
        }

        protected override void SaveData()
        {
            this.Write(this.PublicKeyAlgorithmName);
            this.Write(this.PublicKeyData.GetSshString());
        }
    }
}
