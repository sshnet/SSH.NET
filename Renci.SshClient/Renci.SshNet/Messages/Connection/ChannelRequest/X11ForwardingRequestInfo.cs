namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "x11-req" type channel request information
    /// </summary>
    internal class X11ForwardingRequestInfo : RequestInfo
    {
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
        public string AuthenticationProtocol { get; set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="X11ForwardingRequestInfo"/> class.
        /// </summary>
        public X11ForwardingRequestInfo()
        {
            this.WantReply = true;
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
            this.IsSingleConnection = isSingleConnection;
            this.AuthenticationProtocol = protocol;
            this.AuthenticationCookie = cookie;
            this.ScreenNumber = screenNumber;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.IsSingleConnection = this.ReadBoolean();
            this.AuthenticationProtocol = this.ReadAsciiString();
            this.AuthenticationCookie = this.ReadBinaryString();
            this.ScreenNumber = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.IsSingleConnection);
            this.WriteAscii(this.AuthenticationProtocol);
            this.WriteBinaryString(this.AuthenticationCookie);
            this.Write(this.ScreenNumber);
        }
    }
}
