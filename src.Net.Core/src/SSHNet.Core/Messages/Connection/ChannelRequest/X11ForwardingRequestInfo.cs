namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "x11-req" type channel request information
    /// </summary>
    internal class X11ForwardingRequestInfo : RequestInfo
    {
#if true //old TUNING
        private byte[] _authenticationProtocol;
#endif

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "x11-req";

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
        /// Gets or sets a value indicating whether it is a single connection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if it is a single connection; otherwise, <c>false</c>.
        /// </value>
        public bool IsSingleConnection { get; set; }

        /// <summary>
        /// Gets or sets the authentication protocol.
        /// </summary>
        /// <value>
        /// The authentication protocol.
        /// </value>
#if true //old TUNING
        public string AuthenticationProtocol
        {
            get { return Ascii.GetString(_authenticationProtocol, 0, _authenticationProtocol.Length); }
            private set { _authenticationProtocol = Ascii.GetBytes(value); }
        }
#else
        public string AuthenticationProtocol { get; set; }
#endif

        /// <summary>
        /// Gets or sets the authentication cookie.
        /// </summary>
        /// <value>
        /// The authentication cookie.
        /// </value>
        public byte[] AuthenticationCookie { get; set; }

        /// <summary>
        /// Gets or sets the screen number.
        /// </summary>
        /// <value>
        /// The screen number.
        /// </value>
        public uint ScreenNumber { get; set; }

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
                capacity += 1; // IsSingleConnection
                capacity += 4; // AuthenticationProtocol length
                capacity += _authenticationProtocol.Length; // AuthenticationProtocol
                capacity += 4; // AuthenticationCookie length
                capacity += AuthenticationCookie.Length; // AuthenticationCookie
                capacity += 4; // ScreenNumber
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="X11ForwardingRequestInfo"/> class.
        /// </summary>
        public X11ForwardingRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X11ForwardingRequestInfo"/> class.
        /// </summary>
        /// <param name="isSingleConnection">if set to <c>true</c> it is a single connection.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="screenNumber">The screen number.</param>
        public X11ForwardingRequestInfo(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber)
            : this()
        {
            IsSingleConnection = isSingleConnection;
            AuthenticationProtocol = protocol;
            AuthenticationCookie = cookie;
            ScreenNumber = screenNumber;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            IsSingleConnection = ReadBoolean();
#if true //old TUNING
            _authenticationProtocol = ReadBinary();
            AuthenticationCookie = ReadBinary();
#else
            AuthenticationProtocol = ReadAsciiString();
            AuthenticationCookie = ReadBinaryString();
#endif
            ScreenNumber = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(IsSingleConnection);
#if true //old TUNING
            WriteBinaryString(_authenticationProtocol);
#else
            WriteAscii(AuthenticationProtocol);
#endif
            WriteBinaryString(AuthenticationCookie);
            Write(ScreenNumber);
        }
    }
}
