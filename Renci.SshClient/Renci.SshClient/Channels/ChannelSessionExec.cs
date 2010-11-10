using System;
using System.IO;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelSessionExec : ChannelSession
    {
        private Stream _channelData;

        private Stream _channelExtendedData;

        private Exception _exception;

        private ChannelAsyncResult _asyncResult;

        private AsyncCallback _callback;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        public bool HasError { get; set; }

        public uint ExitStatus { get; private set; }

        public ChannelSessionExec()
            : base()
        {

        }

        internal ChannelAsyncResult BeginExecute(string command, Stream output, Stream extendedOutput, AsyncCallback callback, object state)
        {
            //  Prevent from executing BeginExecute before calling EndExecute
            if (this._asyncResult != null)
            {
                throw new InvalidOperationException("");
            }

            //  Create new AsyncResult object
            this._asyncResult = new ChannelAsyncResult(this)
            {
                AsyncWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset),
                IsCompleted = false,
                AsyncState = state,
            };

            this._callback = callback;
            this._channelData = output;
            this._channelExtendedData = extendedOutput;

            this.Open();

            //  Send channel command request
            this.SendMessage(new ChannelRequestMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                RequestName = ChannelRequestNames.Exec,
                WantReply = false,
                Command = command,
            });

            return _asyncResult;
        }

        internal void EndExecute(IAsyncResult result)
        {
            ChannelAsyncResult channelAsyncResult = result as ChannelAsyncResult;

            if (channelAsyncResult.Channel != this)
            {
                throw new InvalidOperationException("Invalid IAsyncResult parameter");
            }

            //  Make sure that operation completed if not wait for it to finish
            this.WaitHandle(this._asyncResult.AsyncWaitHandle);

            this.Close();

            this._asyncResult = null;

            if (this._exception != null)
            {
                var exception = this._exception;
                this._exception = null; //  Clean exception
                throw exception;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (this._channelData != null)
            {
                this._channelData.Flush();
            }

            if (this._channelExtendedData != null)
            {
                this._channelExtendedData.Flush();
            }

            this._asyncResult.IsCompleted = true;
            if (this._callback != null)
            {
                //  TODO:   Execute this method on different thread since it will be run on message listener
                this._callback(this._asyncResult);
            }
            ((EventWaitHandle)_asyncResult.AsyncWaitHandle).Set();
        }

        protected override void OnData(string data)
        {
            base.OnData(data);

            if (this._channelData != null)
            {
                foreach (var b in data)
                {
                    this._channelData.WriteByte((byte)b);
                }
            }

            if (this._asyncResult != null)
            {
                this._asyncResult.BytesReceived += data.Length;
            }
        }

        protected override void OnExtendedData(string data, uint dataTypeCode)
        {
            base.OnExtendedData(data, dataTypeCode);

            if (this._channelExtendedData != null)
            {
                foreach (var b in data)
                {
                    this._channelExtendedData.WriteByte((byte)b);
                }
            }

            if (dataTypeCode == 1)
            {
                this.HasError = true;
            }
        }

        protected override void OnRequest(ChannelRequestNames requestName, bool wantReply, string command, string subsystemName, uint exitStatus)
        {
            base.OnRequest(requestName, wantReply, command, subsystemName, exitStatus);

            Message replyMessage = new ChannelFailureMessage()
            {
                LocalChannelNumber = this.LocalChannelNumber,
            };

            if (requestName == ChannelRequestNames.ExitStatus)
            {
                this.ExitStatus = exitStatus;

                replyMessage = new ChannelSuccessMessage()
                {
                    LocalChannelNumber = this.LocalChannelNumber,
                };
            }
            else if (requestName == ChannelRequestNames.PseudoTerminal)
            {
                //  TODO:   Check if when this request is received what to do, I suspect we receive this request when no more channel sessions are available
            }
            else
            {
                throw new NotImplementedException(string.Format("Request name {0} is not implemented.", requestName));
            }

            if (wantReply)
            {
                this.SendMessage(replyMessage);
            }
        }


        protected override void OnDisposing()
        {
        }
    }
}
