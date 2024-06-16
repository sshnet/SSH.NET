namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_REQUEST_SUCCESS message.
    /// </summary>
    public class RequestSuccessMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_REQUEST_SUCCESS";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 81;
            }
        }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint? BoundPort { get; private set; }

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

                if (BoundPort.HasValue)
                {
                    capacity += 4; // BoundPort
                }

                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestSuccessMessage"/> class.
        /// </summary>
        public RequestSuccessMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestSuccessMessage"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        public RequestSuccessMessage(uint boundPort)
        {
            BoundPort = boundPort;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            if (!IsEndOfData)
            {
                BoundPort = ReadUInt32();
            }
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            if (BoundPort.HasValue)
            {
                Write(BoundPort.Value);
            }
        }

        internal override void Process(Session session)
        {
            session.OnRequestSuccessReceived(this);
        }
    }
}
