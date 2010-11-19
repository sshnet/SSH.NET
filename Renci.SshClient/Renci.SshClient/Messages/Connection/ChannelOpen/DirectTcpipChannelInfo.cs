namespace Renci.SshClient.Messages.Connection
{
    internal class DirectTcpipChannelInfo : ChannelOpenInfo
    {
        public const string NAME = "direct-tcpip";

        public override string ChannelType
        {
            get { return DirectTcpipChannelInfo.NAME; }
        }

        public string HostToConnect { get; set; }

        public uint PortToConnect { get; set; }

        public string OriginatorAddress { get; set; }

        public uint OriginatorPort { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.HostToConnect = this.ReadString();
            this.PortToConnect = this.ReadUInt32();
            this.OriginatorAddress = this.ReadString();
            this.OriginatorPort = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.HostToConnect);
            this.Write(this.PortToConnect);
            this.Write(this.OriginatorAddress);
            this.Write(this.OriginatorPort);
        }
    }
}
