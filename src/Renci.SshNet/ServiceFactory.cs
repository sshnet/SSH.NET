using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Connection;
using System.Net.Sockets;

namespace Renci.SshNet
{
    /// <summary>
    /// Basic factory for creating new services.
    /// </summary>
    internal partial class ServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Defines the number of times an authentication attempt with any given <see cref="IAuthenticationMethod"/>
        /// can result in <see cref="AuthenticationResult.PartialSuccess"/> before it is disregarded.
        /// </summary>
        private static int PartialSuccessLimit = 5;

        /// <summary>
        /// Creates a <see cref="IClientAuthentication"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IClientAuthentication"/>.
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="socketFactory"/> is <c>null</c>.</exception>
        public ISession CreateSession(ConnectionInfo connectionInfo, ISocketFactory socketFactory)
        {
            return new Session(connectionInfo, this, socketFactory);
        }

        /// <summary>
        /// Creates a new <see cref="ISftpSession"/> in a given <see cref="ISession"/> and with
        /// the specified operation timeout and encoding.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to create the <see cref="ISftpSession"/> in.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or -1 to wait indefinitely.</param>
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

        /// <summary>
        /// Negotiates a key exchange algorithm, and creates a <see cref="IKeyExchange" /> for the negotiated
        /// algorithm.
        /// </summary>
        /// <param name="clientAlgorithms">A <see cref="IDictionary{String, Type}"/> of the key exchange algorithms supported by the client where key is the name of the algorithm, and value is the type implementing this algorithm.</param>
        /// <param name="serverAlgorithms">The names of the key exchange algorithms supported by the SSH server.</param>
        /// <returns>
        /// A <see cref="IKeyExchange"/> that was negotiated between client and server.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="clientAlgorithms"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serverAlgorithms"/> is <c>null</c>.</exception>
        /// <exception cref="SshConnectionException">No key exchange algorithms are supported by both client and server.</exception>
        public IKeyExchange CreateKeyExchange(IDictionary<string, Type> clientAlgorithms, string[] serverAlgorithms)
        {
            if (clientAlgorithms == null)
                throw new ArgumentNullException("clientAlgorithms");
            if (serverAlgorithms == null)
                throw new ArgumentNullException("serverAlgorithms");

            // find an algorithm that is supported by both client and server
            var keyExchangeAlgorithmType = (from c in clientAlgorithms
                                            from s in serverAlgorithms
                                            where s == c.Key
                                            select c.Value).FirstOrDefault();

            if (keyExchangeAlgorithmType == null)
            {
                throw new SshConnectionException("Failed to negotiate key exchange algorithm.", DisconnectReason.KeyExchangeFailed);
            }

            return keyExchangeAlgorithmType.CreateInstance<IKeyExchange>();
        }

        public ISftpFileReader CreateSftpFileReader(string fileName, ISftpSession sftpSession, uint bufferSize)
        {
            const int defaultMaxPendingReads = 3;

            // Issue #292: Avoid overlapping SSH_FXP_OPEN and SSH_FXP_LSTAT requests for the same file as this
            // causes a performance degradation on Sun SSH
            var openAsyncResult = sftpSession.BeginOpen(fileName, Flags.Read, null, null);
            var handle = sftpSession.EndOpen(openAsyncResult);

            var statAsyncResult = sftpSession.BeginLStat(fileName, null, null);

            long? fileSize;
            int maxPendingReads;

            var chunkSize = sftpSession.CalculateOptimalReadLength(bufferSize);

            // fallback to a default maximum of pending reads when remote server does not allow us to obtain
            // the attributes of the file
            try
            {
                var fileAttributes = sftpSession.EndLStat(statAsyncResult);
                fileSize = fileAttributes.Size;
                maxPendingReads = Math.Min(10, (int) Math.Ceiling((double) fileAttributes.Size / chunkSize) + 1);
            }
            catch (SshException ex)
            {
                fileSize = null;
                maxPendingReads = defaultMaxPendingReads;

                DiagnosticAbstraction.Log(string.Format("Failed to obtain size of file. Allowing maximum {0} pending reads: {1}", maxPendingReads, ex));
            }

            return sftpSession.CreateFileReader(handle, sftpSession, chunkSize, maxPendingReads, fileSize);
        }

        public ISftpResponseFactory CreateSftpResponseFactory()
        {
            return new SftpResponseFactory();
        }

        /// <summary>
        /// Creates a shell stream.
        /// </summary>
        /// <param name="session">The SSH session.</param>
        /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal height in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <returns>
        /// The created <see cref="ShellStream"/> instance.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <remarks>
        /// <para>
        /// The <c>TERM</c> environment variable contains an identifier for the text window's capabilities.
        /// You can get a detailed list of these cababilities by using the ‘infocmp’ command.
        /// </para>
        /// <para>
        /// The column/row dimensions override the pixel dimensions(when non-zero). Pixel dimensions refer
        /// to the drawable area of the window.
        /// </para>
        /// </remarks>
        public ShellStream CreateShellStream(ISession session, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues, int bufferSize)
        {
            return new ShellStream(session, terminalName, columns, rows, width, height, terminalModeValues, bufferSize);
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
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            switch (connectionInfo.ProxyType)
            {
                case ProxyTypes.None:
                    return new DirectConnector(socketFactory);
                case ProxyTypes.Socks4:
                    return new Socks4Connector(socketFactory);
                case ProxyTypes.Socks5:
                    return new Socks5Connector(socketFactory);
                case ProxyTypes.Http:
                    return new HttpConnector(socketFactory);
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
