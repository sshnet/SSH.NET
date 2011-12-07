using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

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
