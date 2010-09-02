using System;
using System.Runtime.Serialization;

namespace Renci.SshClient.Common
{
    public class SshException : Exception
    {
        public bool ShouldDisconnect { get; private set; }

        public SshException()
        {
            this.ShouldDisconnect = true;
        }

        public SshException(string message)
            : base(message)
        {
            this.ShouldDisconnect = true;
        }

        public SshException(string message, Exception inner)
            : base(message, inner)
        {
            this.ShouldDisconnect = true;
        }

        public SshException(string message, bool shouldDisconnect)
            : base(message)
        {
            this.ShouldDisconnect = shouldDisconnect;
        }

        public SshException(string message, bool shouldDisconnect, Exception inner)
            : base(message, inner)
        {
            this.ShouldDisconnect = shouldDisconnect;
        }

        // This constructor is needed for serialization.
        protected SshException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Add implementation.
        }

    }
}
