using System;

namespace Renci.SshClient.Messages.Transport
{
    internal class UnimplementedMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Unimplemented; }
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
