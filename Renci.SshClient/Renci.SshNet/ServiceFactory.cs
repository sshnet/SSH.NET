using System;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Basic factory for creating new services.
    /// </summary>
    internal class ServiceFactory : IServiceFactory
    {
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
            return new Session(connectionInfo);
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
    }
}
