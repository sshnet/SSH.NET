
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelOpenDirectTcpIPMessage : ChannelOpenMessage
    {

        public string HostToConnect { get; set; }

        public uint PortToConnect { get; set; }

        public string OriginatorIP { get; set; }

        public uint OriginatorPort { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.HostToConnect = this.ReadString();
            this.PortToConnect = this.ReadUInt32();
            this.OriginatorIP = this.ReadString();
            this.OriginatorPort = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.HostToConnect);
            this.Write(this.PortToConnect);
            this.Write(this.OriginatorIP);
            this.Write(this.OriginatorPort);

        }
    }
}
