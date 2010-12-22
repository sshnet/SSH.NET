namespace Renci.SshClient.Messages.Connection
{
    internal class WindowChangeRequestInfo : RequestInfo
    {
        public const string NAME = "window-change";

        public override string RequestName
        {
            get { return WindowChangeRequestInfo.NAME; }
        }

        public uint Columns { get; private set; }

        public uint Rows { get; private set; }

        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public WindowChangeRequestInfo()
        {
            this.WantReply = false;
        }

        public WindowChangeRequestInfo(uint columns, uint rows, uint width, uint height)
            : this()
        {
            this.Columns = columns;
            this.Rows = rows;
            this.Width = width;
            this.Height = height;
        }

        protected override void LoadData()
        {
            base.LoadData();

            this.Columns = this.ReadUInt32();
            this.Rows = this.ReadUInt32();
            this.Width = this.ReadUInt32();
            this.Height = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Columns);
            this.Write(this.Rows);
            this.Write(this.Width);
            this.Write(this.Height);
        }
    }
}
