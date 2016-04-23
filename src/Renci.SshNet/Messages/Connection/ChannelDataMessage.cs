using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_DATA message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_DATA", MessageNumber)]
    public class ChannelDataMessage : ChannelMessage
    {
        internal const byte MessageNumber = 94;

#if TUNING
        /// <summary>
        /// Gets or sets message data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        /// <remarks>
        /// The actual data to read or write depends on the <see cref="Offset"/> and <see cref="Size"/>.
        /// </remarks>
        public byte[] Data { get; private set; }
#else
        /// <summary>
        /// Gets or sets message data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; protected set; }
#endif

#if TUNING
        /// <summary>
        /// Gets the zero-based offset in <see cref="Data"/> at which the data begins.
        /// </summary>
        /// <value>
        /// The zero-based offset in <see cref="Data"/> at which the data begins.
        /// </value>
        public int Offset { get; set; }

        /// <summary>
        /// Gets the number of bytes of <see cref="Data"/> to read or write.
        /// </summary>
        /// <value>
        /// The number of bytes of <see cref="Data"/> to read or write.
        /// </value>
        public int Size { get; set; }

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
                capacity += Size; // Data
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataMessage"/> class.
        /// </summary>
        public ChannelDataMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="data">Message data.</param>
        public ChannelDataMessage(uint localChannelNumber, byte[] data)
            : base(localChannelNumber)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Data = data;
#if TUNING
            Offset = 0;
            Size = data.Length;
#endif
        }

#if TUNING
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="data">The message data.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="data"/> at which to begin reading or writing data from.</param>
        /// <param name="size">The number of bytes of <paramref name="data"/> to read or write.</param>
        public ChannelDataMessage(uint localChannelNumber, byte[] data, int offset, int size)
            : base(localChannelNumber)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Data = data;
            Offset = offset;
            Size = size;
        }
#endif

        /// <summary>
        /// Loads the data.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            Data = ReadBinary();
            Offset = 0;
            Size = Data.Length;
#else
            Data = ReadBinaryString();
#endif
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
#if TUNING
            WriteBinary(Data, Offset, Size);
#else
            WriteBinaryString(Data);
#endif
        }
    }
}
