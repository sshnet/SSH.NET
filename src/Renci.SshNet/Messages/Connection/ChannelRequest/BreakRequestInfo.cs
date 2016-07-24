namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "break" type channel request information
    /// </summary>
    internal class BreakRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string Name = "break";

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
        /// Gets break length in milliseconds.
        /// </summary>
        public uint BreakLength { get; private set; }

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
                capacity += 4; // BreakLength
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        public BreakRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        /// <param name="breakLength">Length of the break.</param>
        public BreakRequestInfo(uint breakLength)
            : this()
        {
            BreakLength = breakLength;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            BreakLength = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(BreakLength);
        }
    }
}
