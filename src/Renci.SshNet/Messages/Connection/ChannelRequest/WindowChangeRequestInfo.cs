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
        public const string Name = "window-change";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return Name; }
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
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Columns
                capacity += 4; // Rows
                capacity += 4; // Width
                capacity += 4; // Height
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowChangeRequestInfo"/> class.
        /// </summary>
        public WindowChangeRequestInfo()
        {
            WantReply = false;
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
            Columns = columns;
            Rows = rows;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            Columns = ReadUInt32();
            Rows = ReadUInt32();
            Width = ReadUInt32();
            Height = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(Columns);
            Write(Rows);
            Write(Width);
            Write(Height);
        }
    }
}
