using System.Text;
using System.Threading;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelSession : Channel
    {
        private EventWaitHandle _channelEofWaitHandle = new AutoResetEvent(false);
        private StringBuilder _response = new StringBuilder();

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        public ChannelSession(SessionInfo sessionInfo)
            : base(sessionInfo, 0x100000, 0x1000)
        {
        }

        internal string Execute(string command)
        {
            this.Open();

            //  Send channel command request
            this.SendMessage(new ChannelRequestMessage
            {
                ChannelNumber = this.ServerChannelNumber,
                RequestName = RequestNames.Exec,
                WantReply = false,
                Command = command,
            });


            this.SessionInfo.WaitHandle(this._channelEofWaitHandle);

            this.Close();


            return this._response.ToString();
        }

        protected override void OnChannelEof()
        {
            base.OnChannelEof();

            //  TODO:   All wait handles add timeout and then throw an exception or monitor connection closed event
            this._channelEofWaitHandle.Set();
        }

        protected override void OnChannelData(string data)
        {
            base.OnChannelData(data);

            this._response.Append(data);
        }

        private void Init()
        {
            this.ChannelData.Length = 0;
            this.ChannelExtendedData.Length = 0;
        }
    }
}
