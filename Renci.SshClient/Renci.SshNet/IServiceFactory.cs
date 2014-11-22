using System;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet
{
    /// <summary>
    /// Factory for creating new services.
    /// </summary>
    internal partial interface IServiceFactory
    {
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
        /// <param name="operationTimeout">The operation timeout.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// An <see cref="ISftpSession"/>.
        /// </returns>
        ISftpSession CreateSftpSession(ISession session, TimeSpan operationTimeout, Encoding encoding);

        /// <summary>
        /// Create a new <see cref="PipeStream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="PipeStream"/>.
        /// </returns>
        PipeStream CreatePipeStream();
    }
}
