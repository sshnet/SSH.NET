using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.NetConf;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;

namespace Renci.SshNet
{
    /// <summary>
    /// Basic factory for creating new services.
    /// </summary>
    internal sealed partial class ServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Defines the number of times an authentication attempt with any given <see cref="IAuthenticationMethod"/>
        /// can result in <see cref="AuthenticationResult.PartialSuccess"/> before it is disregarded.
        /// </summary>
        private const int PartialSuccessLimit = 5;

        internal ServiceFactory()
        {
        }

        /// <summary>
        /// Creates an <see cref="IClientAuthentication"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IClientAuthentication"/>.
        /// </returns>
        public IClientAuthentication CreateClientAuthentication()
        {
            return new ClientAuthentication(PartialSuccessLimit);
        }

        /// <summary>
        /// Creates a new <see cref="ISession"/> with the specified <see cref="ConnectionInfo"/> and
        /// <see cref="ISocketFactory"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="ConnectionInfo"/> to use for creating a new session.</param>
        /// <param name="socketFactory">A factory to create <see cref="Socket"/> instances.</param>
        /// <returns>
        /// An <see cref="ISession"/> for the specified <see cref="ConnectionInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="socketFactory"/> is <see langword="null"/>.</exception>
        public ISession CreateSession(ConnectionInfo connectionInfo, ISocketFactory socketFactory)
        {
            return new Session(connectionInfo, this, socketFactory);
        }

        /// <summary>
        /// Creates a new <see cref="ISftpSession"/> in a given <see cref="ISession"/> and with
        /// the specified operation timeout and encoding.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to create the <see cref="ISftpSession"/> in.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or <c>-1</c> to wait indefinitely.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="sftpMessageFactory">The factory to use for creating SFTP messages.</param>
        /// <returns>
        /// An <see cref="ISftpSession"/>.
        /// </returns>
        public ISftpSession CreateSftpSession(ISession session, int operationTimeout, Encoding encoding, ISftpResponseFactory sftpMessageFactory)
        {
            return new SftpSession(session, operationTimeout, encoding, sftpMessageFactory);
        }

        /// <summary>
        /// Create a new <see cref="PipeStream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="PipeStream"/>.
        /// </returns>
        public PipeStream CreatePipeStream()
        {
            return new PipeStream();
        }

        /// <inheritdoc/>
        public IKeyExchange CreateKeyExchange(IDictionary<string, Func<IKeyExchange>> clientAlgorithms, string[] serverAlgorithms)
        {
            ThrowHelper.ThrowIfNull(clientAlgorithms);
            ThrowHelper.ThrowIfNull(serverAlgorithms);

            // find an algorithm that is supported by both client and server
            var keyExchangeAlgorithmFactory = (from c in clientAlgorithms
                                               from s in serverAlgorithms
                                               where s == c.Key
                                               select c.Value).FirstOrDefault();

            if (keyExchangeAlgorithmFactory is null)
            {
                throw new SshConnectionException($"No matching key exchange algorithm (server offers {serverAlgorithms.Join(",")})", DisconnectReason.KeyExchangeFailed);
            }

            return keyExchangeAlgorithmFactory();
        }

        /// <summary>
        /// Creates a new <see cref="INetConfSession"/> in a given <see cref="ISession"/>
        /// and with the specified operation timeout.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to create the <see cref="INetConfSession"/> in.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or <c>-1</c> to wait indefinitely.</param>
        /// <returns>
        /// An <see cref="INetConfSession"/>.
        /// </returns>
        public INetConfSession CreateNetConfSession(ISession session, int operationTimeout)
        {
            return new NetConfSession(session, operationTimeout);
        }

        /// <summary>
        /// Creates a new <see cref="ISftpResponseFactory"/> instance.
        /// </summary>
        /// <returns>
        /// An <see cref="ISftpResponseFactory"/>.
        /// </returns>
        public ISftpResponseFactory CreateSftpResponseFactory()
        {
            return new SftpResponseFactory();
        }

        /// <inheritdoc/>
        public ShellStream CreateShellStream(ISession session, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues, int bufferSize)
        {
            return new ShellStream(session, terminalName, columns, rows, width, height, terminalModeValues, bufferSize);
        }

        /// <inheritdoc/>
        public ShellStream CreateShellStreamNoTerminal(ISession session, int bufferSize)
        {
            return new ShellStream(session, bufferSize);
        }

        /// <summary>
        /// Creates an <see cref="IRemotePathTransformation"/> that encloses a path in double quotes, and escapes
        /// any embedded double quote with a backslash.
        /// </summary>
        /// <returns>
        /// An <see cref="IRemotePathTransformation"/> that encloses a path in double quotes, and escapes any
        /// embedded double quote with a backslash.
        /// with a shell.
        /// </returns>
        public IRemotePathTransformation CreateRemotePathDoubleQuoteTransformation()
        {
            return RemotePathTransformation.DoubleQuote;
        }

        /// <summary>
        /// Creates an <see cref="IConnector"/> that can be used to establish a connection
        /// to the server identified by the specified <paramref name="connectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="IConnectionInfo"/> detailing the server to establish a connection to.</param>
        /// <param name="socketFactory">A factory to create <see cref="Socket"/> instances.</param>
        /// <returns>
        /// An <see cref="IConnector"/> that can be used to establish a connection to the
        /// server identified by the specified <paramref name="connectionInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="socketFactory"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="IConnectionInfo.ProxyType"/> value of <paramref name="connectionInfo"/> is not supported.</exception>
        public IConnector CreateConnector(IConnectionInfo connectionInfo, ISocketFactory socketFactory)
        {
            ThrowHelper.ThrowIfNull(connectionInfo);
            ThrowHelper.ThrowIfNull(socketFactory);

            var loggerFactory = connectionInfo.LoggerFactory ?? SshNetLoggingConfiguration.LoggerFactory;

            switch (connectionInfo.ProxyType)
            {
                case ProxyTypes.None:
                    return new DirectConnector(socketFactory, loggerFactory);
                case ProxyTypes.Socks4:
                    return new Socks4Connector(socketFactory, loggerFactory);
                case ProxyTypes.Socks5:
                    return new Socks5Connector(socketFactory, loggerFactory);
                case ProxyTypes.Http:
                    return new HttpConnector(socketFactory, loggerFactory);
                default:
                    throw new NotSupportedException(string.Format("ProxyTypes '{0}' is not supported.", connectionInfo.ProxyType));
            }
        }

        /// <summary>
        /// Creates an <see cref="IProtocolVersionExchange"/> that deals with the SSH protocol
        /// version exchange.
        /// </summary>
        /// <returns>
        /// An <see cref="IProtocolVersionExchange"/>.
        /// </returns>
        public IProtocolVersionExchange CreateProtocolVersionExchange()
        {
            return new ProtocolVersionExchange();
        }

        /// <summary>
        /// Creates a factory to create <see cref="Socket"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="ISocketFactory"/>.
        /// </returns>
        public ISocketFactory CreateSocketFactory()
        {
            return new SocketFactory();
        }
    }
}
