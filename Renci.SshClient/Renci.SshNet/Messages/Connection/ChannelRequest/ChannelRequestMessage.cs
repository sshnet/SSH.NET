using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_REQUEST", 98)]
    public class ChannelRequestMessage : ChannelMessage
    {
        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public string RequestName
        {
            get
            {
                return this.Info.RequestName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the reply is needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if reply is needed; otherwise, <c>false</c>.
        /// </value>
        public bool WantReply
        {
            get
            {
                return this.Info.WantReply;
            }
        }

        /// <summary>
        /// Gets channel request information.
        /// </summary>
        public RequestInfo Info { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestMessage"/> class.
        /// </summary>
        public ChannelRequestMessage()
        {
            //  Required for dynamically loading request type when it comes from the server
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestMessage"/> class.
        /// </summary>
        /// <param name="localChannelName">Name of the local channel.</param>
        /// <param name="info">The info.</param>
        public ChannelRequestMessage(uint localChannelName, RequestInfo info)
        {
            this.LocalChannelNumber = localChannelName;
            this.Info = info;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
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
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Request '{0}' is not supported.", requestName));
            }

            this.Info.Load(bytes);
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.RequestName);
            this.Write(this.Info.GetBytes());
        }
    }
}
