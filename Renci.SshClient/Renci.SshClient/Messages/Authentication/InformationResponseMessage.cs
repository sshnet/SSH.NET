using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    [Message("SSH_MSG_USERAUTH_INFO_RESPONSE", 61)]
    internal class InformationResponseMessage : Message
    {
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
