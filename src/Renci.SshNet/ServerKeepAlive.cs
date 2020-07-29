using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    /// <summary>
    /// Responds to keep alive packets from the server (https://tools.ietf.org/html/rfc4254#section-4)
    /// </summary>
    class ServerKeepAlive {
        internal static void ServerKeepAliveHandler(Object sender, EventArgs e)
        {
            var session = (ISession) sender;
            session.SendMessage(new RequestSuccessMessage());
        }
    }
}