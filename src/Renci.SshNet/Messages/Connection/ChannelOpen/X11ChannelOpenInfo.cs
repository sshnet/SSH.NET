using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "x11" channel type
    /// </summary>
    internal class X11ChannelOpenInfo : ChannelOpenInfo
    {
#if TUNING
        private byte[] _originatorAddress;
#endif

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "x11";

        /// <summary>
        /// Gets the type of the channel to open.
        /// </summary>
        /// <value>
        /// The type of the channel to open.
        /// </value>
        public override string ChannelType
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
#if TUNING
        public string OriginatorAddress
        {
            get { return Utf8.GetString(_originatorAddress, 0, _originatorAddress.Length); }
            private set { _originatorAddress = Utf8.GetBytes(value); }
        }
#else
        public string OriginatorAddress { get; private set; }
#endif

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

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
                capacity += 4; // OriginatorAddress length
                capacity += _originatorAddress.Length; // OriginatorAddress
                capacity += 4; // OriginatorPort
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="X11ChannelOpenInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public X11ChannelOpenInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X11ChannelOpenInfo"/> class with the
        /// specified originator address and port.
        /// </summary>
        /// <param name="originatorAddress">The originator address.</param>
        /// <param name="originatorPort">The originator port.</param>
        public X11ChannelOpenInfo(string originatorAddress, uint originatorPort)
        {
            OriginatorAddress = originatorAddress;
            OriginatorPort = originatorPort;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if TUNING
            _originatorAddress = ReadBinary();
#else
            OriginatorAddress = ReadString();
#endif
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_originatorAddress);
#else
            Write(OriginatorAddress);
#endif
            Write(OriginatorPort);
        }
    }
}
