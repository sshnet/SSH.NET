using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    [Message("SSH_MSG_USERAUTH_PK_OK", 60)]
    internal class PublicKeyMessage : Message
    {
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
