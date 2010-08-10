using System;

namespace Renci.SshClient.Messages.Transport
{
    internal class IgnoreMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Ignore; }
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
