namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "xon-xoff" type channel request information
    /// </summary>
    internal class XonXoffRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request type
        /// </summary>
        public const string Name = "xon-xoff";

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
        /// Gets or sets a value indicating whether client can do.
        /// </summary>
        /// <value>
        ///   <c>true</c> if client can do; otherwise, <c>false</c>.
        /// </value>
        public bool ClientCanDo { get; set; }

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
                capacity += 1; // ClientCanDo
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XonXoffRequestInfo"/> class.
        /// </summary>
        public XonXoffRequestInfo()
        {
            WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XonXoffRequestInfo"/> class.
        /// </summary>
        /// <param name="clientCanDo">if set to <c>true</c> [client can do].</param>
        public XonXoffRequestInfo(bool clientCanDo)
            : this()
        {
            ClientCanDo = clientCanDo;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            ClientCanDo = ReadBoolean();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(ClientCanDo);
        }
    }
}
