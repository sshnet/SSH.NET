namespace Renci.SshClient.Messages.Connection
{
    internal class X11ChannelOpenInfo : ChannelOpenInfo
    {
        public const string NAME = "x11";

        public override string ChannelType
        {
            get { return X11ChannelOpenInfo.NAME; }
        }

        public string OriginatorAddress { get; set; }

        public uint OriginatorPort { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.OriginatorAddress = this.ReadString();
            this.OriginatorPort = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.OriginatorAddress);
            this.Write(this.OriginatorPort);
        }
    }
}
