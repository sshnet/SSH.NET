using System;
using Renci.SshClient.Channels;

namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelOpenMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpen; }
        }

        public ChannelTypes ChannelType { get; set; }

        public uint InitialWindowSize { get; set; }

        public uint MaximumPacketSize { get; set; }

        public string ConnectedAddress { get; set; }

        public UInt32 ConnectedPort { get; set; }

        public string OriginatorAddress { get; set; }

        public UInt32 OriginatorPort { get; set; }

        protected override void LoadData()
        {
            var channelName = this.ReadString();
            this.LocalChannelNumber = this.ReadUInt32();
            this.InitialWindowSize = this.ReadUInt32();
            this.MaximumPacketSize = this.ReadUInt32();

            if (channelName == "session")
            {
                this.ChannelType = ChannelTypes.Session;
            }
            else if (channelName == "x11")
            {
                this.ChannelType = ChannelTypes.X11;
            }
            else if (channelName == "forwarded-tcpip")
            {
                this.ChannelType = ChannelTypes.ForwardedTcpip;
                this.ConnectedAddress = this.ReadString();
                this.ConnectedPort = this.ReadUInt32();
                this.OriginatorAddress = this.ReadString();
                this.OriginatorPort = this.ReadUInt32();
            }
            else if (channelName == "direct-tcpip")
            {
            }
            else
            {
                throw new NotSupportedException(string.Format("Channel type '{0}' is not supported.", channelName));
            }
        }

        protected override void SaveData()
        {

            switch (this.ChannelType)
            {
                case ChannelTypes.Session:
                    this.Write("session");
                    break;
                case ChannelTypes.X11:
                    this.Write("x11");
                    break;
                case ChannelTypes.ForwardedTcpip:
                    this.Write("forwarded-tcpip");
                    break;
                case ChannelTypes.DirectTcpip:
                    this.Write("direct-tcpip");
                    break;
                default:
                    throw new NotSupportedException(string.Format("Channel type '{0}' is not supported.", this.ChannelType));
            }
            this.Write(this.LocalChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
        }
    }
}
