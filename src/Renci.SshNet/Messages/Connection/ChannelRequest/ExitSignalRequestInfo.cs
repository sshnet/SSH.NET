namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exit-signal" type channel request information
    /// </summary>
    internal class ExitSignalRequestInfo : RequestInfo
    {
#if TUNING
        private byte[] _signalName;
        private byte[] _errorMessage;
        private byte[] _language;
#endif

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "exit-signal";

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
#if TUNING
        public string SignalName
        {
            get { return Ascii.GetString(_signalName, 0, _signalName.Length); }
            private set { _signalName = Ascii.GetBytes(value); }
        }
#else
        public string SignalName { get; private set; }
#endif

        /// <summary>
        /// Gets a value indicating whether core is dumped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if core is dumped; otherwise, <c>false</c>.
        /// </value>
        public bool CoreDumped { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
#if TUNING
        public string ErrorMessage
        {
            get { return Utf8.GetString(_errorMessage, 0, _errorMessage.Length); }
            private set { _errorMessage = Utf8.GetBytes(value); }
        }
#else
        public string ErrorMessage { get; private set; }
#endif

        /// <summary>
        /// Gets message language.
        /// </summary>
#if TUNING
        public string Language
        {
            get { return Utf8.GetString(_language, 0, _language.Length); }
            private set { _language = Utf8.GetBytes(value); }
        }
#else
        public string Language { get; private set; }
#endif

#if TUNING
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // SignalName length
                capacity += _signalName.Length; // SignalName
                capacity += 1; // CoreDumped
                capacity += 4; // ErrorMessage length
                capacity += _errorMessage.Length; // ErrorMessage
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitSignalRequestInfo"/> class.
        /// </summary>
        public ExitSignalRequestInfo()
        {
            WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitSignalRequestInfo"/> class.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <param name="coreDumped">if set to <c>true</c> then core is dumped.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="language">The language.</param>
        public ExitSignalRequestInfo(string signalName, bool coreDumped, string errorMessage, string language)
            : this()
        {
            SignalName = signalName;
            CoreDumped = coreDumped;
            ErrorMessage = errorMessage;
            Language = language;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if TUNING
            _signalName = ReadBinary();
#else
            SignalName = ReadAsciiString();
#endif
            CoreDumped = ReadBoolean();
#if TUNING
            _errorMessage = ReadBinary();
            _language = ReadBinary();
#else
            ErrorMessage = ReadString();
            Language = ReadString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_signalName);
#else
            WriteAscii(SignalName);
#endif
            Write(CoreDumped);
#if TUNING
            Write(_errorMessage);
            Write(_language);
#else
            Write(ErrorMessage);
            Write(Language);
#endif
        }

    }
}
