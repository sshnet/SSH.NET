using System;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshConnectionException : SshException
    {
        public DisconnectReasons DisconnectReason { get; private set; }

        public SshConnectionException(string message)
            : base(message)
        {
            this.DisconnectReason = DisconnectReasons.None;
        }

        public SshConnectionException(string message, DisconnectReasons disconnectReasonCode)
            : base(message)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        public SshConnectionException(string message, DisconnectReasons disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            this.DisconnectReason = disconnectReasonCode;
        }
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
