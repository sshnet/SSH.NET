namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "window-change" type channel request information
    /// </summary>
    internal class WindowChangeRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channe request name
        /// </summary>
        public const string NAME = "window-change";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public uint Columns { get; private set; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        public uint Rows { get; private set; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowChangeRequestInfo"/> class.
        /// </summary>
        public WindowChangeRequestInfo()
        {
            this.WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowChangeRequestInfo"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public WindowChangeRequestInfo(uint columns, uint rows, uint width, uint height)
            : this()
        {
            this.Columns = columns;
            this.Rows = rows;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.Columns = this.ReadUInt32();
            this.Rows = this.ReadUInt32();
            this.Width = this.ReadUInt32();
            this.Height = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
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
