using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_OPEN message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_OPEN", 90)]
    public class ChannelOpenMessage : ChannelMessage
    {
        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public string ChannelType
        {
            get
            {
                return this.Info.ChannelType;
            }
        }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        /// <value>
        /// The initial size of the window.
        /// </value>
        public uint InitialWindowSize { get; private set; }

        /// <summary>
        /// Gets the maximum size of the packet.
        /// </summary>
        /// <value>
        /// The maximum size of the packet.
        /// </value>
        public uint MaximumPacketSize { get; private set; }

        /// <summary>
        /// Gets channel specific open information.
        /// </summary>
        public ChannelOpenInfo Info { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenMessage"/> class.
        /// </summary>
        public ChannelOpenMessage()
        {
            //  Required for dynamicly loading request type when it comes from the server
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenMessage"/> class.
        /// </summary>
        /// <param name="channelNumber">The channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        /// <param name="info">The info.</param>
        public ChannelOpenMessage(uint channelNumber, uint initialWindowSize, uint maximumPacketSize, ChannelOpenInfo info)
        {
            this.LocalChannelNumber = channelNumber;
            this.InitialWindowSize = initialWindowSize;
            this.MaximumPacketSize = maximumPacketSize;
            this.Info = info;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            var channelName = this.ReadAsciiString();
            this.LocalChannelNumber = this.ReadUInt32();
            this.InitialWindowSize = this.ReadUInt32();
            this.MaximumPacketSize = this.ReadUInt32();
            var bytes = this.ReadBytes();

            if (channelName == SessionChannelOpenInfo.NAME)
            {
                this.Info = new SessionChannelOpenInfo();
            }
            else if (channelName == X11ChannelOpenInfo.NAME)
            {
                this.Info = new X11ChannelOpenInfo();
            }
            else if (channelName == DirectTcpipChannelInfo.NAME)
            {
                this.Info = new DirectTcpipChannelInfo();
            }
            else if (channelName == ForwardedTcpipChannelInfo.NAME)
            {
                this.Info = new ForwardedTcpipChannelInfo();
            }
            else
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Channel type '{0}' is not supported.", channelName));
            }

            this.Info.Load(bytes);

        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.WriteAscii(this.ChannelType);
            this.Write(this.LocalChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
            this.Write(this.Info.GetBytes());
        }
    }
}
