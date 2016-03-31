namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "signal" type channel request information
    /// </summary>
    internal class SignalRequestInfo : RequestInfo
    {
#if true //old TUNING
        private byte[] _signalName;
#endif

        /// <summary>
        /// Channel request name.
        /// </summary>
        public const string NAME = "signal";

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
        /// Gets the name of the signal.
        /// </summary>
        /// <value>
        /// The name of the signal.
        /// </value>
#if true //old TUNING
        public string SignalName
        {
            get { return Ascii.GetString(_signalName, 0, _signalName.Length); }
            private set { _signalName = Ascii.GetBytes(value); }
        }
#else
        public string SignalName { get; private set; }
#endif

#if true //old TUNING
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
                capacity += 4; // SignalName length
                capacity += _signalName.Length; // SignalName
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRequestInfo"/> class.
        /// </summary>
        public SignalRequestInfo()
        {
            WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRequestInfo"/> class.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        public SignalRequestInfo(string signalName)
            : this()
        {
            SignalName = signalName;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if true //old TUNING
            _signalName = ReadBinary();
#else
            SignalName = ReadAsciiString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if true //old TUNING
            WriteBinaryString(_signalName);
#else
            WriteAscii(SignalName);
#endif
        }
    }
}
