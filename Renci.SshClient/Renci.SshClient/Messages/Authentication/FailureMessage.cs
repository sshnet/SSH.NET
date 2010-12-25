using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    [Message("SSH_MSG_USERAUTH_FAILURE", 51)]
    public class FailureMessage : Message
    {
        public IEnumerable<string> AllowedAuthentications { get; set; }

        public string Message { get; private set; }

        public bool PartialSuccess { get; private set; }

        protected override void LoadData()
        {
            this.AllowedAuthentications = this.ReadNamesList();
            this.PartialSuccess = this.ReadBoolean();
            if (this.PartialSuccess)
            {
                this.Message = string.Join(",", this.AllowedAuthentications);
            }
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
