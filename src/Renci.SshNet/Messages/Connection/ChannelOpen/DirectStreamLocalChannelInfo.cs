using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "direct-streamlocal@openssh.com" channel type
    /// </summary>
    internal class DirectStreamLocalChannelInfo : ChannelOpenInfo
    {
        private byte[] _socketPath;

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "direct-streamlocal@openssh.com";

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
        /// Gets the path to connect.
        /// </summary>
        public string SocketPath
        {
            get { return Utf8.GetString(_socketPath, 0, _socketPath.Length); }
            private set { _socketPath = Utf8.GetBytes(value); }
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
                capacity += 4; // SocketPath length
                capacity += SocketPath.Length; // SocketPath
                capacity += 4; // Reserved1 length
                capacity += 0; // Reserved1
                capacity += 4; // Reserved2
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectStreamLocalChannelInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public DirectStreamLocalChannelInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectStreamLocalChannelInfo"/> class.
        /// </summary>
        /// <param name="socketPath">The path to connect.</param>
        public DirectStreamLocalChannelInfo(string socketPath)
        {
            SocketPath = socketPath;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _socketPath = ReadBinary();
            ReadBinary(); // Reserved1
            ReadUInt32(); // Reserved2
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_socketPath);
            Write(string.Empty); // Reserved1
            Write((uint)0);      // Reserved2
        }
    }
}
