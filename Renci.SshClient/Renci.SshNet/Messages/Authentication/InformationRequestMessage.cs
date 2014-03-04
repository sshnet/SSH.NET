using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
