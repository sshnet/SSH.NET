using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Authentication
{
    public class FailureMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationFailure; }
        }

        public IEnumerable<string> AllowedAuthentications { get; set; }

        public string Message { get; set; }

        public bool PartialSuccess { get; set; }

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
