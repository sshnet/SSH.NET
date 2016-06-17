using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "x11" channel type
    /// </summary>
    internal class X11ChannelOpenInfo : ChannelOpenInfo
    {
        private byte[] _originatorAddress;

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string Name = "x11";

        /// <summary>
        /// Gets the type of the channel to open.
        /// </summary>
        /// <value>
        /// The type of the channel to open.
        /// </value>
        public override string ChannelType
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
        public string OriginatorAddress
        {
            get { return Utf8.GetString(_originatorAddress, 0, _originatorAddress.Length); }
            private set { _originatorAddress = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

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

            _originatorAddress = ReadBinary();
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_originatorAddress);
            Write(OriginatorPort);
        }
    }
}
