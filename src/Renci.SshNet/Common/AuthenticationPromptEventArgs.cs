using System.Collections.Generic;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="KeyboardInteractiveConnectionInfo.AuthenticationPrompt"/> event.
    /// </summary>
    public class AuthenticationPromptEventArgs : AuthenticationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationPromptEventArgs"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="language">The language.</param>
        /// <param name="prompts">The information request prompts.</param>
        public AuthenticationPromptEventArgs(string username, string instruction, string language, IReadOnlyList<AuthenticationPrompt> prompts)
            : base(username)
        {
            Instruction = instruction;
            Language = language;
            Prompts = prompts;
        }

        /// <summary>
        /// Gets prompt language.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Gets prompt instruction.
        /// </summary>
        public string Instruction { get; }

        /// <summary>
        /// Gets server information request prompts.
        /// </summary>
        public IReadOnlyList<AuthenticationPrompt> Prompts { get; }
    }
}
