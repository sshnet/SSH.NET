using System;
using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelExec : Channel
    {
        private EventWaitHandle _channelExecutionWaitHandle = new AutoResetEvent(false);

        private Stream _channelData;

        private Stream _channelExtendedData;

        private Exception _exception;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        public ChannelExec(Session session)
            : base(session, 0x100000, 0x1000)
        {
        }

        internal void Execute(string command, Stream output, Stream extendedOutput)
        {
            this._channelData = output;
            this._channelExtendedData = extendedOutput;

            this.Open();

            //  Send channel command request
            this.SendMessage(new ChannelRequestMessage
            {
                ChannelNumber = this.ServerChannelNumber,
                RequestName = RequestNames.Exec,
                WantReply = false,
                Command = command,
            });


            this.Session.WaitHandle(this._channelExecutionWaitHandle);

            this.Close();

            if (this._exception != null)
            {
                var exception = this._exception;
                this._exception = null; //  Clean exception
                throw exception;
            }
        }

        protected override void OnChannelEof()
        {
            base.OnChannelEof();

            this._channelExecutionWaitHandle.Set();
        }

        protected override void OnChannelFailed(uint reasonCode, string description)
        {
            base.OnChannelFailed(reasonCode, description);
            this._exception = new InvalidOperationException(string.Format("Channel failed to open. Code: {0}, Reason {1}", reasonCode, description));
            this._channelExecutionWaitHandle.Set();
        }

        protected override void OnChannelData(string data)
        {
            base.OnChannelData(data);

            if (this._channelData != null)
            {
                this._channelData.Write(data.GetSshBytes().ToArray(), 0, data.Length);
            }
        }

        protected override void OnChannelExtendedData(string data, uint dataTypeCode)
        {
            base.OnChannelExtendedData(data, dataTypeCode);

            //  TODO:   dataTypeCode curentlyu ignored
            if (this._channelExtendedData != null)
            {
                this._channelExtendedData.Write(data.GetSshBytes().ToArray(), 0, data.Length);
            }
        }

        private void Init()
        {
            this.ChannelData.Length = 0;
            this.ChannelExtendedData.Length = 0;
        }
    }
}
