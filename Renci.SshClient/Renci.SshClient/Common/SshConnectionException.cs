using System;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshConnectionException : SshException
    {
        public DisconnectReasons DisconnectReason { get; private set; }

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
    }
}
