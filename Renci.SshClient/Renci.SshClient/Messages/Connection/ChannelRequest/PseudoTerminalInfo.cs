namespace Renci.SshClient.Messages.Connection
{
    internal class PseudoTerminalRequestInfo : RequestInfo
    {
        public const string NAME = "pty-req";

        public override string RequestName
        {
            get { return PseudoTerminalRequestInfo.NAME; }
        }

        public string EnvironmentVariable { get; set; }

        public uint Columns { get; set; }

        public uint Rows { get; set; }

        public uint PixelWidth { get; set; }

        public uint PixelHeight { get; set; }

        public string TerminalMode { get; set; }

        public PseudoTerminalRequestInfo()
        {
            this.WantReply = true;
        }

        public PseudoTerminalRequestInfo(string environmentVariable, uint columns, uint rows, uint width, uint height, string terminalMode)
            : this()
        {
            this.EnvironmentVariable = environmentVariable;
            this.Columns = columns;
            this.Rows = rows;
            this.PixelWidth = width;
            this.PixelHeight = height;
            this.TerminalMode = terminalMode;
        }

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
