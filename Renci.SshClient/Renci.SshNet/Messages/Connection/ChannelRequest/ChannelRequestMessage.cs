namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_REQUEST", 98)]
    public class ChannelRequestMessage : ChannelMessage
    {
        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public string RequestName { get; private set; }

        /// <summary>
        /// Gets channel request data.
        /// </summary>
        public byte[] RequestData { get; private set; }

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
        /// <param name="localChannelName">Name of the local channel.</param>
        /// <param name="info">The info.</param>
        public ChannelRequestMessage(uint localChannelName, RequestInfo info)
        {
            LocalChannelNumber = localChannelName;
            RequestName = info.RequestName;
            RequestData = info.GetBytes();
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            RequestName = ReadAsciiString();
            RequestData = ReadBytes();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteAscii(RequestName);
            Write(RequestData);
        }
    }
}
