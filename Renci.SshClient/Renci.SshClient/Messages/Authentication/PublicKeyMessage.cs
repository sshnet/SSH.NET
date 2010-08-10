using System;

namespace Renci.SshClient.Messages.Authentication
{
    internal class PublicKeyMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationPublicKey; }
        }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
