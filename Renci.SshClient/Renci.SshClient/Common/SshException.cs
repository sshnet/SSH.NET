using System;
using System.Runtime.Serialization;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshException : Exception
    {
        public bool ShouldDisconnect { get; private set; }

        public DisconnectReasonCodes DisconnectReasonCode { get; private set; }

        public SshException()
        {
            this.ShouldDisconnect = true;
            this.DisconnectReasonCode = DisconnectReasonCodes.ByApplication;
        }

        public SshException(string message)
            : base(message)
        {
            this.ShouldDisconnect = true;
            this.DisconnectReasonCode = DisconnectReasonCodes.ByApplication;
        }

        public SshException(string message, Exception inner)
            : base(message, inner)
        {
            this.ShouldDisconnect = true;
            this.DisconnectReasonCode = DisconnectReasonCodes.ByApplication;
        }

        public SshException(string message, bool shouldDisconnect)
            : base(message)
        {
            this.ShouldDisconnect = shouldDisconnect;
            this.DisconnectReasonCode = DisconnectReasonCodes.ByApplication;
        }

        public SshException(string message, bool shouldDisconnect, Exception inner)
            : base(message, inner)
        {
            this.ShouldDisconnect = shouldDisconnect;
            this.DisconnectReasonCode = DisconnectReasonCodes.ByApplication;
        }

        public SshException(string message, bool shouldDisconnect, DisconnectReasonCodes disconnectReasonCode)
            : base(message)
        {
            this.ShouldDisconnect = shouldDisconnect;
            this.DisconnectReasonCode = disconnectReasonCode;
        }

        public SshException(string message, bool shouldDisconnect, DisconnectReasonCodes disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            this.ShouldDisconnect = shouldDisconnect;
            this.DisconnectReasonCode = disconnectReasonCode;
        }

        protected SshException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
