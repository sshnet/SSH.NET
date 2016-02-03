
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestPseudoTerminalMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "pty-req";

        public string EnvironmentVariable { get; set; }

        public uint Columns { get; set; }

        public uint Rows { get; set; }

        public uint PixelWidth { get; set; }

        public uint PixelHeight { get; set; }

        public string TerminalMode { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.EnvironmentVariable = this.ReadString();
            this.Columns = this.ReadUInt32();
            this.Rows = this.ReadUInt32();
            this.PixelWidth = this.ReadUInt32();
            this.PixelHeight = this.ReadUInt32();
            this.TerminalMode = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;
            base.SaveData();
            this.Write(this.EnvironmentVariable);
            this.Write(this.Columns);
            this.Write(this.Rows);
            this.Write(this.Rows);
            this.Write(this.PixelHeight);
            this.Write(this.TerminalMode);

        }
    }
}
