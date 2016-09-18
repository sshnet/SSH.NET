using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_OPEN message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_OPEN", MessageNumber)]
    public class ChannelOpenMessage : Message
    {
        internal const byte MessageNumber = 90;

        private byte[] _infoBytes;

        /// <summary>
        /// Gets the type of the channel as ASCII encoded byte array.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public byte[] ChannelType { get; private set; }

        /// <summary>
        /// Gets or sets the local channel number.
        /// </summary>
        /// <value>
        /// The local channel number.
        /// </value>
        public uint LocalChannelNumber { get; protected set; }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        /// <value>
        /// The initial size of the window.
        /// </value>
        public uint InitialWindowSize { get; private set; }

        /// <summary>
        /// Gets the maximum size of the packet.
        /// </summary>
        /// <value>
        /// The maximum size of the packet.
        /// </value>
        public uint MaximumPacketSize { get; private set; }

        /// <summary>
        /// Gets channel specific open information.
        /// </summary>
        public ChannelOpenInfo Info { get; private set; }

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
                capacity += 4; // ChannelType length
                capacity += ChannelType.Length; // ChannelType
                capacity += 4; // LocalChannelNumber
                capacity += 4; // InitialWindowSize
                capacity += 4; // MaximumPacketSize
                capacity += _infoBytes.Length; // Info
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenMessage"/> class.
        /// </summary>
        public ChannelOpenMessage()
        {
            //  Required for dynamicly loading request type when it comes from the server
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenMessage"/> class.
        /// </summary>
        /// <param name="channelNumber">The channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        /// <param name="info">Information specific to the type of the channel to open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public ChannelOpenMessage(uint channelNumber, uint initialWindowSize, uint maximumPacketSize, ChannelOpenInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            ChannelType = Ascii.GetBytes(info.ChannelType);
            LocalChannelNumber = channelNumber;
            InitialWindowSize = initialWindowSize;
            MaximumPacketSize = maximumPacketSize;
            Info = info;
            _infoBytes = info.GetBytes();
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ChannelType = ReadBinary();
            LocalChannelNumber = ReadUInt32();
            InitialWindowSize = ReadUInt32();
            MaximumPacketSize = ReadUInt32();
            _infoBytes = ReadBytes();

            var channelName = Ascii.GetString(ChannelType, 0, ChannelType.Length);

            switch (channelName)
            {
                case SessionChannelOpenInfo.Name:
                    Info = new SessionChannelOpenInfo(_infoBytes);
                    break;
                case X11ChannelOpenInfo.Name:
                    Info = new X11ChannelOpenInfo(_infoBytes);
                    break;
                case DirectTcpipChannelInfo.NAME:
                    Info = new DirectTcpipChannelInfo(_infoBytes);
                    break;
                case ForwardedTcpipChannelInfo.NAME:
                    Info = new ForwardedTcpipChannelInfo(_infoBytes);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Channel type '{0}' is not supported.", channelName));
            }
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(ChannelType);
            Write(LocalChannelNumber);
            Write(InitialWindowSize);
            Write(MaximumPacketSize);
            Write(_infoBytes);
        }

        internal override void Process(Session session)
        {
            session.OnChannelOpenReceived(this);
        }
    }
}
