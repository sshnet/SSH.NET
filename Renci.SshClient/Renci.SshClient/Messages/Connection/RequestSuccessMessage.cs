
using System;
namespace Renci.SshClient.Messages.Connection
{
    internal class RequestSuccessMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.RequestSuccess; }
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
