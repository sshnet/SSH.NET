using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    public class AuthenticationPromptEventArgs : AuthenticationEventArgs
    {
        public string Language { get; private set; }

        public string Instruction { get; private set; }

        public IEnumerable<AuthenticationPrompt> Prompts { get; private set; }

        public AuthenticationPromptEventArgs(string username, string instruction, string language, IEnumerable<AuthenticationPrompt> prompts)
            : base(username)
        {
            this.Instruction = instruction;
            this.Language = language;
            this.Prompts = prompts;
        }
    }
}
