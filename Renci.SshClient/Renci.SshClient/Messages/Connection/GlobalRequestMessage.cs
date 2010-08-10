using System;

namespace Renci.SshClient.Messages.Connection
{
    internal class GlobalRequestMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.GlobalRequest; }
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
