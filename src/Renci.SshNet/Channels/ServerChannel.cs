using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    internal abstract class ServerChannel : Channel
    {
        /// <summary>
        /// Initializes a new <see cref="ServerChannel"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="remoteWindowSize">The window size of the remote party.</param>
        /// <param name="remotePacketSize">The maximum size of a data packet that we can send to the remote party.</param>
        protected ServerChannel(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize, uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
            InitializeRemoteInfo(remoteChannelNumber, remoteWindowSize, remotePacketSize);
        }

        protected void SendMessage(ChannelOpenConfirmationMessage message)
        {
            //  No need to check whether channel is open when trying to open a channel
            Session.SendMessage(message);

            //  When we act as server, consider the channel open when we've sent the
            // confirmation message to the peer
            IsOpen = true;
        }
    }
}
