using System.Net.Sockets;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-streamlocal@openssh.com" SSH channel.
    /// </summary>
    internal class ChannelDirectStreamLocal : ChannelDirectBase, IChannelDirectStreamLocal
    {
        /// <summary>
        /// Initializes a new <see cref="ChannelDirectStreamLocal"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        public ChannelDirectStreamLocal(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
        }

        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.DirectStreamLocal; }
        }

        public void Open(string socketPath, IForwardedPort forwardedPort, Socket socket)
        {
            base.Open(new DirectStreamLocalChannelInfo(socketPath), forwardedPort, socket);
        }
    }
}
