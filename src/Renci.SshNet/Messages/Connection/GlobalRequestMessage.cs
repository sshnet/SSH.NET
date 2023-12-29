﻿namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_GLOBAL_REQUEST message.
    /// </summary>
    public class GlobalRequestMessage : Message
    {
        private byte[] _requestName;

        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_GLOBAL_REQUEST";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 80;
            }
        }

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public string RequestName
        {
            get { return Ascii.GetString(_requestName, 0, _requestName.Length); }
        }

        /// <summary>
        /// Gets a value indicating whether message reply should be sent..
        /// </summary>
        /// <value>
        /// <see langword="true"/> if message reply should be sent; otherwise, <see langword="false"/>.
        /// </value>
        public bool WantReply { get; private set; }

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
                capacity += _requestName.Length; // RequestName
                capacity += 1; // WantReply
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        public GlobalRequestMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <see langword="true"/> [want reply].</param>
        internal GlobalRequestMessage(byte[] requestName, bool wantReply)
        {
            _requestName = requestName;
            WantReply = wantReply;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            _requestName = ReadBinary();
            WantReply = ReadBoolean();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(_requestName);
            Write(WantReply);
        }

        internal override void Process(Session session)
        {
            session.OnGlobalRequestReceived(this);
        }
    }
}
