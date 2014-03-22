using System;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    internal abstract class ServerChannel : Channel
    {
        internal void Initialize(Session session, uint localWindowSize, uint localPacketSize, uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
        {
            Initialize(session, localWindowSize, localPacketSize);
            InitializeRemoteInfo(remoteChannelNumber, remoteWindowSize, remotePacketSize);
            //Session.ChannelOpenReceived += OnChannelOpen;
        }

        //private void OnChannelOpen(object sender, MessageEventArgs<ChannelOpenMessage> e)
        //{
        //    var channelOpenMessage = e.Message;

        //    if (channelOpenMessage.LocalChannelNumber == LocalChannelNumber)
        //    {
        //        _remoteChannelNumber = channelOpenMessage.LocalChannelNumber;
        //        RemoteWindowSize = channelOpenMessage.InitialWindowSize;
        //        _remotePacketSize = channelOpenMessage.MaximumPacketSize;
        //        OnOpen(e.Message.Info);
        //    }
        //}

        protected void SendMessage(ChannelOpenConfirmationMessage message)
        {
            //  No need to check whether channel is open when trying to open a channel
            Session.SendMessage(message);

            //  When we act as server, consider the channel open when we've sent the
            // confirmation message to the peer
            IsOpen = true;
        }

        ///// <summary>
        ///// Called when channel need to be open on the client.
        ///// </summary>
        ///// <param name="info">Channel open information.</param>
        //protected virtual void OnOpen(ChannelOpenInfo info)
        //{
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var session = Session;
                if (session != null)
                {
                    //session.ChannelOpenReceived -= OnChannelOpen;
                }
            }

            base.Dispose(disposing);
        }
    }
}
