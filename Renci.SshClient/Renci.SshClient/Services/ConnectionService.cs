using System;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Services
{
    internal class ConnectionService : Service
    {
        public override ServiceNames ServiceName
        {
            get { throw new NotImplementedException(); }
        }

        public ConnectionService(Session session)
            : base(session)
        {

        }
    }
}
