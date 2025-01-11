using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_IGNORE message.
    /// </summary>
    public class IgnoreMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_IGNORE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets ignore message data if this message has been initialised
        /// with data to be sent. Otherwise, returns an empty array.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        public IgnoreMessage()
        {
            Data = Array.Empty<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public IgnoreMessage(byte[] data)
        {
            ThrowHelper.ThrowIfNull(data);

            Data = data;
        }

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
                capacity += 4; // Data length
                capacity += Data.Length; // Data
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            // Do nothing - this data is supposed to be ignored.
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(Data);
        }

        internal override void Process(Session session)
        {
            session.OnIgnoreReceived(this);
        }
    }
}
