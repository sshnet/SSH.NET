namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_REQUEST", 98)]
    public class ChannelRequestMessage : ChannelMessage
    {
#if TUNING
        private string _requestName;
        private byte[] _requestNameBytes;
#endif

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
#if TUNING
        public string RequestName
        {
            get { return _requestName; }
            private set
            {
                _requestName = value;
                _requestNameBytes = Ascii.GetBytes(value);
            }
        }
#else
        public string RequestName { get; private set; }
#endif

        /// <summary>
        /// Gets channel request data.
        /// </summary>
        public byte[] RequestData { get; private set; }

#if TUNING
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
                capacity += 4; // RequestName length
                capacity += _requestNameBytes.Length; // RequestName
                capacity += RequestData.Length; // RequestData
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestMessage"/> class.
        /// </summary>
        public ChannelRequestMessage()
        {
            //  Required for dynamically loading request type when it comes from the server
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="info">The info.</param>
        public ChannelRequestMessage(uint localChannelNumber, RequestInfo info)
            : base(localChannelNumber)
        {
            RequestName = info.RequestName;
            RequestData = info.GetBytes();
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if TUNING
            _requestNameBytes = ReadBinary();
            _requestName = Ascii.GetString(_requestNameBytes);
#else
            RequestName = ReadAsciiString();
#endif
            RequestData = ReadBytes();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_requestNameBytes);
#else
            WriteAscii(RequestName);
#endif
            Write(RequestData);
        }
    }
}
