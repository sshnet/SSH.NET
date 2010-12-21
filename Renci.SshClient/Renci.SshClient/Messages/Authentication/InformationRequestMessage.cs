using System;
using System.Linq;
using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages.Authentication
{
    internal class InformationRequestMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationInformationRequest; }
        }

        public string Name { get; set; }

        public string Instruction { get; set; }

        public string Language { get; set; }

        public IEnumerable<AuthenticationPrompt> Prompts { get; set; }

        protected override void LoadData()
        {
            this.Name = this.ReadString();
            this.Instruction = this.ReadString();
            this.Language = this.ReadString();

            var numOfPrompts = this.ReadUInt32();
            var prompts = new List<AuthenticationPrompt>();

            for (int i = 0; i < numOfPrompts; i++)
            {
                var prompt = this.ReadString();
                var echo = this.ReadBoolean();
                prompts.Add(new AuthenticationPrompt(i, echo, prompt));
            }

            this.Prompts = prompts;
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
