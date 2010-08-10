using System;

namespace Renci.SshClient.Messages.Connection
{
    internal class RequestFailureMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.RequestFailure; }
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
