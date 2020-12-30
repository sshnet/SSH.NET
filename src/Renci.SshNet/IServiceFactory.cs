using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;

namespace Renci.SshNet
{
    /// <summary>
    /// Factory for creating new services.
    /// </summary>
    internal partial interface IServiceFactory
    {
        IClientAuthentication CreateClientAuthentication();

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
        ISession CreateSession(ConnectionInfo connectionInfo, ISocketFactory socketFactory);

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
        ISftpSession CreateSftpSession(ISession session, int operationTimeout, Encoding encoding, ISftpResponseFactory sftpMessageFactory);

        /// <summary>
        /// Create a new <see cref="PipeStream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="PipeStream"/>.
        /// </returns>
        PipeStream CreatePipeStream();

        /// <summary>
        /// Negotiates a key exchange algorithm, and creates a <see cref="IKeyExchange" /> for the negotiated
        /// algorithm.
        /// </summary>
        /// <param name="clientAlgorithms">A <see cref="IDictionary{String, Type}"/> of the key exchange algorithms supported by the client where the key is the name of the algorithm, and the value is the type implementing this algorithm.</param>
        /// <param name="serverAlgorithms">The names of the key exchange algorithms supported by the SSH server.</param>
        /// <returns>
        /// A <see cref="IKeyExchange"/> that was negotiated between client and server.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="clientAlgorithms"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serverAlgorithms"/> is <c>null</c>.</exception>
        /// <exception cref="SshConnectionException">No key exchange algorithm is supported by both client and server.</exception>
        IKeyExchange CreateKeyExchange(IDictionary<string, Type> clientAlgorithms, string[] serverAlgorithms);

        ISftpFileReader CreateSftpFileReader(string fileName, ISftpSession sftpSession, uint bufferSize);

        ISftpResponseFactory CreateSftpResponseFactory();

        /// <summary>
        /// Creates a shell stream.
        /// </summary>
        /// <param name="session">The SSH session.</param>
        /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal height in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
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
        ShellStream CreateShellStream(ISession session,
                                      string terminalName,
                                      uint columns,
                                      uint rows,
                                      uint width,
                                      uint height,
                                      IDictionary<TerminalModes, uint> terminalModeValues,
                                      int bufferSize);

        /// <summary>
        /// Creates an <see cref="IRemotePathTransformation"/> that encloses a path in double quotes, and escapes
        /// any embedded double quote with a backslash.
        /// </summary>
        /// <returns>
        /// An <see cref="IRemotePathTransformation"/> that encloses a path in double quotes, and escapes any
        /// embedded double quote with a backslash.
        /// with a shell.
        /// </returns>
        IRemotePathTransformation CreateRemotePathDoubleQuoteTransformation();

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
        IConnector CreateConnector(IConnectionInfo connectionInfo, ISocketFactory socketFactory);

        /// <summary>
        /// Creates an <see cref="IProtocolVersionExchange"/> that deals with the SSH protocol
        /// version exchange.
        /// </summary>
        /// <returns>
        /// An <see cref="IProtocolVersionExchange"/>.
        /// </returns>
        IProtocolVersionExchange CreateProtocolVersionExchange();

        /// <summary>
        /// Creates a factory to create <see cref="Socket"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="ISocketFactory"/>.
        /// </returns>
        ISocketFactory CreateSocketFactory();
    }
}
