using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Common
{
    public class AuthenticationEventArgs : EventArgs
    {
        public string BannerMessage { get; private set; }

        public string Language { get; private set; }

        public string Instruction { get; private set; }

        public IEnumerable<AuthenticationPrompt> Prompts { get; private set; }

        public AuthenticationEventArgs(string message, string language)
        {
            this.BannerMessage = message;
            this.Language = language;
        }

        public AuthenticationEventArgs(string instruction, string language, IEnumerable<AuthenticationPrompt> prompts)
        {
            this.Instruction = instruction;
            this.Language = language;
            this.Prompts = prompts;
        }
    }
}
