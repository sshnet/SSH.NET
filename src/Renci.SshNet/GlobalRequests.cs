using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    /// <summary>
    /// Responds to SSH Global Requests (https://tools.ietf.org/html/rfc4254#section-4)
    /// </summary>
    class GlobalRequests {
        /// <summary>
        /// Handles Global Request Events. By default, it replies with a failure message which is appropriate to 
        /// support server keepalives (https://tools.ietf.org/html/rfc4254#section-4)
        /// </summary>
        internal static void GlobalRequestHandler(Object sender, EventArgs e)
        {
            var session = (ISession) sender;
            session.SendMessage(new RequestFailureMessage());
        }
    }
}