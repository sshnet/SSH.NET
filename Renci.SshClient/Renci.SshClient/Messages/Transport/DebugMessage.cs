using System;

namespace Renci.SshClient.Messages.Transport
{
    internal class DebugMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Debug; }
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
