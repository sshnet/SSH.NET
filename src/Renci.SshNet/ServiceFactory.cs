using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet
{
    /// <summary>
    /// Basic factory for creating new services.
    /// </summary>
    internal partial class ServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Creates a <see cref="IClientAuthentication"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IClientAuthentication"/>.
        /// </returns>
        public IClientAuthentication CreateClientAuthentication()
        {
            return new ClientAuthentication();
        }

        /// <summary>
        /// Creates a new <see cref="ISession"/> with the specified <see cref="ConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="ConnectionInfo"/> to use for creating a new session.</param>
        /// <returns>
        /// An <see cref="ISession"/> for the specified <see cref="ConnectionInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        public ISession CreateSession(ConnectionInfo connectionInfo)
        {
            return new Session(connectionInfo, this);
        }

        /// <summary>
        /// Creates a new <see cref="ISftpSession"/> in a given <see cref="ISession"/> and with
        /// the specified operation timeout and encoding.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to create the <see cref="ISftpSession"/> in.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or -1 to wait indefinitely.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// An <see cref="ISftpSession"/>.
        /// </returns>
        public ISftpSession CreateSftpSession(ISession session, int operationTimeout, Encoding encoding)
        {
            return new SftpSession(session, operationTimeout, encoding);
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
            const int DefaultMaxPendingReads = 3;

            var openAsyncResult = sftpSession.BeginOpen(fileName, Flags.Read, null, null);
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
                maxPendingReads = DefaultMaxPendingReads;

                DiagnosticAbstraction.Log(string.Format("Failed to obtain size of file. Allowing maximum {0} pending reads: {1}", maxPendingReads, ex));
            }

            var handle = sftpSession.EndOpen(openAsyncResult);

            return sftpSession.CreateFileReader(handle, sftpSession, chunkSize, maxPendingReads, fileSize);
        }
    }
}
