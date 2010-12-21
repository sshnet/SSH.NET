using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    internal class InformationResponseMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationInformationResponse; }
        }

        public IList<string> Responses { get; private set; }

        public InformationResponseMessage()
        {
            this.Responses = new List<string>();
        }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            this.Write((UInt32)this.Responses.Count);
            foreach (var response in this.Responses)
            {
                this.Write(response);
            }
        }
    }
}
