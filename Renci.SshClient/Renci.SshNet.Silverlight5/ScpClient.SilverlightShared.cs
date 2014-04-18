using Renci.SshNet.Channels;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient
    {
        partial void SendData(ChannelSession channel, string command)
        {
            this.Session.SendMessage(new ChannelDataMessage(channel.RemoteChannelNumber, System.Text.Encoding.UTF8.GetBytes(command)));
        }
    }
}
