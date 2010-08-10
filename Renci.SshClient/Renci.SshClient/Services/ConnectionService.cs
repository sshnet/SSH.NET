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

        public ConnectionService(SessionInfo sessionInfo)
            : base(sessionInfo)
        {

        }

        //public override void Request()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void Accept()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
