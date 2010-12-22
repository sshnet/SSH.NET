namespace Renci.SshClient.Messages.Connection
{
    internal class DirectTcpipChannelInfo : ChannelOpenInfo
    {
        public const string NAME = "direct-tcpip";

        public override string ChannelType
        {
            get { return DirectTcpipChannelInfo.NAME; }
        }

        public string HostToConnect { get; private set; }

        public uint PortToConnect { get; private set; }

        public string OriginatorAddress { get; private set; }

        public uint OriginatorPort { get; private set; }

        public DirectTcpipChannelInfo()
        {

        }
        
        public DirectTcpipChannelInfo(string hostToConnect, uint portToConnect, string originatorAddress, uint originatorPort)
        {
            this.HostToConnect = hostToConnect;
            this.PortToConnect = portToConnect;
            this.OriginatorAddress = originatorAddress;
            this.OriginatorPort = originatorPort;
        }

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
