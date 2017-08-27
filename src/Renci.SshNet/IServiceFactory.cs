using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet.Common;
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
        /// Creates a new <see cref="ISession"/> with the specified <see cref="ConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="ConnectionInfo"/> to use for creating a new session.</param>
        /// <returns>
        /// An <see cref="ISession"/> for the specified <see cref="ConnectionInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        ISession CreateSession(ConnectionInfo connectionInfo);

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
        /// Creates an <see cref="IRemotePathTransformation"/>  that quotes a path in a way to be suitable
        /// to be used with a shell.
        /// </summary>
        /// <returns>
        /// An <see cref="IRemotePathTransformation"/>  that quotes a path in a way to be suitable to be used
        /// with a shell.
        /// </returns>
        IRemotePathTransformation CreateRemotePathQuoteTransformation();
    }
}
