using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if FEATURE_TAP
using System.Threading;
using System.Threading.Tasks;
#endif

using Renci.SshNet.Channels;

namespace Renci.SshNet.Connection
{
    class SshConnector : ConnectorBase
    {
        private ISession _jumpSession;
        private JumpChannel _jumpChannel;

        public SshConnector(IServiceFactory serviceFactory, ISocketFactory socketFactory):
            base (serviceFactory, socketFactory)
        {
        }

        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (connectionInfo.GetType() != typeof(ConnectionInfo))
                throw new ArgumentException("Expecting connectionInfo to be of type ConnectionInfo");

            _jumpSession = new Session((ConnectionInfo)connectionInfo, ServiceFactory, SocketFactory);
            _jumpSession.Connect();
            _jumpChannel = new JumpChannel(_jumpSession, connectionInfo.Host, (uint)connectionInfo.Port);
            return _jumpChannel.Connect();
        }

#if FEATURE_TAP
        public override async Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (connectionInfo.GetType() != typeof(ConnectionInfo))
                throw new ArgumentException("Expecting connectionInfo to be of type ConnectionInfo");

            _jumpSession = new Session((ConnectionInfo)connectionInfo, ServiceFactory, SocketFactory);
            await _jumpSession.ConnectAsync(cancellationToken).ConfigureAwait(false);
            _jumpChannel = new JumpChannel(_jumpSession, connectionInfo.Host, (uint)connectionInfo.Port);
            return _jumpChannel.Connect();
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposedValue)
            {
                var jumpChannel = _jumpChannel;
                if (jumpChannel != null)
                {
                    jumpChannel.Dispose();
                    _jumpChannel = null;
                }

                var jumpSession = _jumpSession;
                if (jumpSession != null)
                {
                    jumpSession.Dispose();
                    _jumpSession = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
