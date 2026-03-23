using System;

namespace Renci.SshNet
{
    /// <inheritdoc cref="V2.SshClient" />
    [Obsolete($"Use {nameof(V2.SshClient)} instead.")]
    public class SshClient : V2.SshClient
    {
        /// <inheritdoc />
        public SshClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
        }

        /// <inheritdoc />
        public SshClient(string host, string username, string password)
            : base(host, username, password)
        {
        }

        /// <inheritdoc />
        public SshClient(string host, string username, params IPrivateKeySource[] keyFiles)
            : base(host, username, keyFiles)
        {
        }

        /// <inheritdoc />
        public SshClient(string host, int port, string username, string password)
            : base(host, port, username, password)
        {
        }

        /// <inheritdoc />
        public SshClient(string host, int port, string username, params IPrivateKeySource[] keyFiles)
            : base(host, port, username, keyFiles)
        {
        }

        /// <inheritdoc />
        internal SshClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }
    }
}
