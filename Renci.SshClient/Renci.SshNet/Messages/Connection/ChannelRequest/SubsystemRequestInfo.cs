using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "subsystem" type channel request information
    /// </summary>
    internal class SubsystemRequestInfo : RequestInfo
    {
#if TUNING
        private byte[] _subsystemName;
#endif

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "subsystem";

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
        /// Gets the name of the subsystem.
        /// </summary>
        /// <value>
        /// The name of the subsystem.
        /// </value>
#if TUNING
        public string SubsystemName
        {
            get { return Ascii.GetString(_subsystemName); }
            private set { _subsystemName = Ascii.GetBytes(value); }
        }
#else
        public string SubsystemName { get; private set; }
#endif

#if TUNING
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
#endif

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

#if TUNING
            _subsystemName = ReadBinary();
#else
            SubsystemName = ReadAsciiString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_subsystemName);
#else
            WriteAscii(SubsystemName);
#endif
        }
    }
}
