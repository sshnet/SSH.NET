using System;
using System.Runtime.Serialization;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshException : Exception
    {
        public DisconnectReasons DisconnectReason { get; private set; }

        public uint ExitStatus { get; private set; }

        public SshException()
        {
        }

        public SshException(string message)
            : base(message)
        {
        }

        public SshException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public SshException(string message, uint exitStatus)
            : base(message)
        {
            this.ExitStatus = exitStatus;
        }

        public SshException(string message, uint exitStatus, Exception inner)
            : base(message, inner)
        {
            this.ExitStatus = exitStatus;
        }

        public SshException(string message, DisconnectReasons disconnectReasonCode)
            : base(message)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        public SshException(string message, DisconnectReasons disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        protected SshException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
