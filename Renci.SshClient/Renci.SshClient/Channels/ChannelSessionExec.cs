using System;
using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelSessionExec : ChannelSession
    {
        /// <summary>
        /// Holds channel data stream
        /// </summary>
        private Stream _channelData;

        /// <summary>
        /// Holds channel extended data stream
        /// </summary>
        private Stream _channelExtendedData;

        private ChannelAsyncResult _asyncResult;

        private AsyncCallback _callback;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this channel has error.
        /// </summary>
        /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
        public bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the exit status.
        /// </summary>
        /// <value>The exit status.</value>
        public uint ExitStatus { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSessionExec"/> class.
        /// </summary>
        public ChannelSessionExec()
            : base()
        {

        }

        /// <summary>
        /// Begins the execute.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Ends the execute.
        /// </summary>
        /// <param name="result">The result.</param>
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
        }

        /// <summary>
        /// Called when channel is closed
        /// </summary>
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

        /// <summary>
        /// Called when channel receives data.
        /// </summary>
        /// <param name="data">The data.</param>
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

        /// <summary>
        /// Called when channel receives extended data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="dataTypeCode">The data type code.</param>
        protected override void OnExtendedData(string data, uint dataTypeCode)
        {
            base.OnExtendedData(data, dataTypeCode);

            if (this._channelExtendedData != null)
            {
                this._channelExtendedData.Write(data.GetSshBytes().ToArray(), 0, data.Length);
                //foreach (var b in data)
                //{
                //    this._channelExtendedData.WriteByte((byte)b);
                //}
            }

            if (dataTypeCode == 1)
            {
                this.HasError = true;
            }
        }

        /// <summary>
        /// Called when channel request command is called.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <c>true</c> then need to send reply to server.</param>
        /// <param name="command">The command.</param>
        /// <param name="subsystemName">Name of the subsystem.</param>
        /// <param name="exitStatus">The exit status.</param>
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

        /// <summary>
        /// Called when object is being disposed.
        /// </summary>
        protected override void OnDisposing()
        {
        }
    }
}
