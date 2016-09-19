using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_INFO_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_INFO_REQUEST", 60)]
    internal class InformationRequestMessage : Message
    {
        /// <summary>
        /// Gets information request name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets information request instruction.
        /// </summary>
        public string Instruction { get; private set; }

        /// <summary>
        /// Gets information request language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Gets information request prompts.
        /// </summary>
        public IEnumerable<AuthenticationPrompt> Prompts { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            Name = ReadString(Encoding.UTF8);
            Instruction = ReadString(Encoding.UTF8);

            // language tag as defined in rfc3066:
            // Language tags may always be presented using the characters A-Z, a-z, 0 - 9 and HYPHEN-MINUS
            Language = ReadString(Ascii);

            var numOfPrompts = ReadUInt32();
            var prompts = new List<AuthenticationPrompt>();

            for (var i = 0; i < numOfPrompts; i++)
            {
                var prompt = ReadString(Encoding.UTF8);
                var echo = ReadBoolean();
                prompts.Add(new AuthenticationPrompt(i, echo, prompt));
            }

            Prompts = prompts;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            throw new NotImplementedException();
        }

        internal override void Process(Session session)
        {
            session.OnUserAuthenticationInformationRequestReceived(this);
        }
    }
}
