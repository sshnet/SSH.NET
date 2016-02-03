using System;
using System.Collections.Generic;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;

namespace Renci.SshClient
{
    public delegate void SendMessage(Message message);

    internal class SessionInfo
    {
        private SendMessage _sendMessage;

        private EventWaitHandle _disconnectWaitHandle = new AutoResetEvent(false);

        private int _waitTimeout;

        public ConnectionInfo ConnectionInfo { get; private set; }

        public IEnumerable<byte> SessionId { get; set; }

        public string ServerVersion { get; set; }

        public string ClientVersion { get; set; }

        public SessionInfo(SendMessage sendMessage, ConnectionInfo connectionInfo, int waitTimeout)
        {
            this._sendMessage = sendMessage;
            this._waitTimeout = waitTimeout;
            this.ConnectionInfo = connectionInfo;
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void RaiseMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(sender, args);
            }
        }

        public void SendMessage(Message message)
        {
            this._sendMessage(message);
        }

        public void Disconnect()
        {
            this._disconnectWaitHandle.Set();
        }

        public void WaitHandle(EventWaitHandle waitHandle)
        {
            var waitHandles = new EventWaitHandle[]
                {
                    this._disconnectWaitHandle,
                    waitHandle,
                };
            var index = EventWaitHandle.WaitAny(waitHandles);

            //var index = EventWaitHandle.WaitAny(waitHandles, this._waitTimeout);

            //if (index > waitHandles.Length)
            //{
            //  //  TODO:   Issue timeout disconnect message if approapriate
            //    throw new TimeoutException();
            //}
        }
    }
}
