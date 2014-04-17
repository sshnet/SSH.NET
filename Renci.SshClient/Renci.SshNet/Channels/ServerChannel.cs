using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    internal abstract class ServerChannel : Channel
    {
        internal void Initialize(Session session, uint localWindowSize, uint localPacketSize, uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
        {
            Initialize(session, localWindowSize, localPacketSize);
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
