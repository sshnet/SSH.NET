namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "subsystem" type channel request information
    /// </summary>
    internal class SubsystemRequestInfo : RequestInfo
    {
        private byte[] _subsystemName;

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string Name = "subsystem";

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
        /// Gets the name of the subsystem.
        /// </summary>
        /// <value>
        /// The name of the subsystem.
        /// </value>
        public string SubsystemName
        {
            get { return Ascii.GetString(_subsystemName, 0, _subsystemName.Length); }
            private set { _subsystemName = Ascii.GetBytes(value); }
        }

        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // SubsystemName length
                capacity += _subsystemName.Length; // SubsystemName
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsystemRequestInfo"/> class.
        /// </summary>
        public SubsystemRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsystemRequestInfo"/> class.
        /// </summary>
        /// <param name="subsystem">The subsystem.</param>
        public SubsystemRequestInfo(string subsystem)
            : this()
        {
            SubsystemName = subsystem;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _subsystemName = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_subsystemName);
        }
    }
}
