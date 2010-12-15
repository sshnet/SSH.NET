using System;

namespace Renci.SshClient.Messages.Connection
{
    public class ChannelOpenMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpen; }
        }

        public string ChannelType
        {
            get
            {
                return this.Info.ChannelType;
            }
        }

        public uint InitialWindowSize { get; private set; }

        public uint MaximumPacketSize { get; private set; }

        public ChannelOpenInfo Info { get; private set; }

        public ChannelOpenMessage()
        {
            //  Required for dynamicly loading request type when it comes from the server
        }

        public ChannelOpenMessage(uint channelNumber, uint initialWindowSize, uint maximumPacketSize, ChannelOpenInfo info)
        {
            this.LocalChannelNumber = channelNumber;
            this.InitialWindowSize = initialWindowSize;
            this.MaximumPacketSize = maximumPacketSize;
            this.Info = info;
        }

        protected override void LoadData()
        {
            var channelName = this.ReadString();
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
                throw new NotSupportedException(string.Format("Channel type '{0}' is not supported.", channelName));
            }

            this.Info.Load(bytes);

        }

        protected override void SaveData()
        {
            this.Write(this.ChannelType);
            this.Write(this.LocalChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
            this.Write(this.Info.GetBytes());
        }
    }
}
