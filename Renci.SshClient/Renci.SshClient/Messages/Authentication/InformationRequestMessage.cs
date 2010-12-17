using System;
using System.Linq;
using System.Collections.Generic;

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

        public IEnumerable<PromptEcho> Prompts { get; set; }

        protected override void LoadData()
        {
            this.Name = this.ReadString();
            this.Instruction = this.ReadString();
            this.Language = this.ReadString();

            var numOfPrompts = this.ReadUInt32();
            var prompts = new List<PromptEcho>();

            for (int i = 0; i < numOfPrompts; i++)
			{
                prompts.Add(new PromptEcho
                    {
                        Prompt = this.ReadString(),
                        Echo = this.ReadBoolean(),
                    });            
			}

            this.Prompts = prompts;
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }

        public class PromptEcho
        {
            public string Prompt { get; set; }

            public bool Echo { get; set; }
        }
    }
}
