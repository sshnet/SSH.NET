using System.Text;
using Renci.SshNet.Channels;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient
    {
        partial void SendData(IChannelSession channel, string command)
        {
            channel.SendData(Encoding.UTF8.GetBytes(command));
        }
    }
}
