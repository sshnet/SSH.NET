using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "keyboard-interactive" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessageKeyboardInteractive : RequestMessage
    {
        /// <summary>
        /// Gets message language.
        /// </summary>
        public byte[] Language { get; private set; }

        /// <summary>
        /// Gets authentication sub methods.
        /// </summary>
        public byte[] SubMethods { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Language length
                capacity += Language.Length; // Language
                capacity += 4; // SubMethods length
                capacity += SubMethods.Length; // SubMethods
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageKeyboardInteractive"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        public RequestMessageKeyboardInteractive(ServiceName serviceName, string username)
            : base(serviceName, username, "keyboard-interactive")
        {
            Language = Array<byte>.Empty;
            SubMethods = Array<byte>.Empty;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(Language);
            WriteBinaryString(SubMethods);
        }
    }
}
