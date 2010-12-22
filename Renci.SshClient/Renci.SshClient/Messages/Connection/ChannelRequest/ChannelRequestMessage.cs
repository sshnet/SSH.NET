using System;

namespace Renci.SshClient.Messages.Connection
{
    public class ChannelRequestMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelRequest; }
        }

        public string RequestName
        {
            get
            {
                return this.Info.RequestName;
            }
        }

        public bool WantReply
        {
            get
            {
                return this.Info.WantReply;
            }
        }

        public RequestInfo Info { get; private set; }

        public ChannelRequestMessage()
        {
            //  Required for dynamicly loading request type when it comes from the server
        }

        public ChannelRequestMessage(uint localChannelName, RequestInfo info)
        {
            this.LocalChannelNumber = localChannelName;
            this.Info = info;
        }

        protected override void LoadData()
        {
            base.LoadData();

            var requestName = this.ReadString();
            var bytes = this.ReadBytes();

            if (requestName == EnvironmentVariableRequestInfo.NAME)
            {
                this.Info = new EnvironmentVariableRequestInfo();
            }
            else if (requestName == ExecRequestInfo.NAME)
            {
                this.Info = new ExecRequestInfo();
            }
            else if (requestName == ExitSignalRequestInfo.NAME)
            {
                this.Info = new ExitSignalRequestInfo();
            }
            else if (requestName == ExitStatusRequestInfo.NAME)
            {
                this.Info = new ExitStatusRequestInfo();
            }
            else if (requestName == PseudoTerminalRequestInfo.NAME)
            {
                this.Info = new PseudoTerminalRequestInfo();
            }
            else if (requestName == ShellRequestInfo.NAME)
            {
                this.Info = new ShellRequestInfo();
            }
            else if (requestName == SignalRequestInfo.NAME)
            {
                this.Info = new SignalRequestInfo();
            }
            else if (requestName == SubsystemRequestInfo.NAME)
            {
                this.Info = new SubsystemRequestInfo();
            }
            else if (requestName == WindowChangeRequestInfo.NAME)
            {
                this.Info = new WindowChangeRequestInfo();
            }
            else if (requestName == X11ForwardingRequestInfo.NAME)
            {
                this.Info = new X11ForwardingRequestInfo();
            }
            else if (requestName == XonXoffRequestInfo.NAME)
            {
                this.Info = new XonXoffRequestInfo();
            }
            else
            {
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", requestName));
            }

            this.Info.Load(bytes);
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.RequestName);
            this.Write(this.Info.GetBytes());
        }
    }
}
