using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "session" channel type
    /// </summary>
    internal sealed class SessionChannelOpenInfo : ChannelOpenInfo
    {
        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string Name = "session";

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
        /// Initializes a new instance of the <see cref="SessionChannelOpenInfo"/> class.
        /// </summary>
        public SessionChannelOpenInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionChannelOpenInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public SessionChannelOpenInfo(byte[] data)
        {
            Load(data);
        }
    }
}
